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
using QuilvianSystemBackend.Responses;
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

            // VAL-68 dan VAL-69 (LAB-DEC-057). Ditegakkan di sini — sesudah daftar
            // pemeriksaannya sah, sebelum satu baris pun dibuat — dengan alasan yang sama
            // seperti pemeriksaan bahan di bawah: permintaan yang ditolak tidak boleh
            // meninggalkan wadah setengah jadi.
            await EnsureOrderedProcedureGuardAsync(order, procedureIds, cancellationToken);

            // VAL-51 .. VAL-57. Bahan yang dibawa wadah — jenisnya dan volumenya — diperiksa di
            // sini, sebelum satu baris pun dibuat, supaya permintaan yang ditolak tidak
            // meninggalkan wadah setengah jadi.
            var material = await ResolveSpecimenMaterialAsync(request, cancellationToken);

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            // VAL-58 dan VAL-59. Sejak r16, pembanding VAL-59 datang dari permintaan: waktu
            // pengambilan YANG DINYATAKAN PETUGAS, bukan cap waktu server yang pada jalur
            // rujukan luar justru lebih akhir daripada waktu kedatangan sampel.
            //
            // Ketika ruas itu kosong, VAL-59 tidak menyala — dan itu disengaja, bukan celah.
            var physicallyReceivedAt = ResolvePhysicalReceipt(
                request.PhysicallyReceivedAt,
                request.CollectedAt,
                now);

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
                material,
                physicallyReceivedAt,
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
                    specimen.SpecimenTypeId,
                    specimen.VolumeUnitId,
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
            var dibuat = new List<LabExamination>(procedures.Count);

            foreach (var procedure in procedures)
            {
                var tariff = await ResolveTariffAsync(procedure.Id, now, cancellationToken);

                var examination = new LabExamination
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
                };

                _dbContext.LabExaminations.Add(examination);
                dibuat.Add(examination);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            // AC-91. Penandaan sengaja diletakkan di sini, bukan di PlanAsync, supaya jalur
            // pengambilan ulang ikut tertandai: wadah pengganti memanggil method yang sama, dan
            // tautannya berpindah ke baris pemeriksaan yang benar-benar akan dikerjakan.
            await MarkOrderedProceduresFulfilledAsync(order, dibuat, actorUserId, now, cancellationToken);
        }

        /// <summary>
        /// <c>VAL-68</c> dan <c>VAL-69</c> — wadah hanya boleh memuat pemeriksaan yang memang
        /// dipesan untuk pasien itu (<c>LAB-DEC-057</c>).
        ///
        /// <b>Keduanya aditif, bukan pengetatan diam-diam.</b> Penjagaan ini hanya berlaku bila
        /// pesanannya memiliki baris <see cref="LabOrderedProcedure"/>. Pesanan lama tidak
        /// memilikinya — tabelnya baru berdiri lewat <c>BE-LAB-26</c> — sehingga bagi mereka
        /// jalur ini berperilaku persis seperti sebelum penjagaan ini ada. Bagian 12.5 arsitektur
        /// backend menuliskannya sebagai keputusan, bukan sebagai kelonggaran sementara:
        /// <c>BE-LAB-21</c> sudah menunjukkan berapa mahal harga ruas wajib yang ditambahkan
        /// diam-diam ke endpoint yang sedang dipakai.
        /// </summary>
        private async Task EnsureOrderedProcedureGuardAsync(
            LabOrder order,
            IReadOnlyList<Guid> procedureIds,
            CancellationToken cancellationToken)
        {
            var terpesan = await _dbContext.LabOrderedProcedures
                .AsNoTracking()
                .Where(x => x.LabOrderId == order.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);

            // Inilah satu-satunya pintu keluar yang membuat penjagaan ini aditif.
            if (terpesan.Count == 0) return;

            // VAL-68. Permintaan yang sudah dibatalkan diperlakukan sebagai TIDAK ada pada
            // daftar: ia pernah dipesan, tetapi tidak lagi diminta. Membiarkannya lolos berarti
            // permintaan yang sudah dicabut hidup kembali lewat pintu wadah, tanpa ada yang
            // memutuskannya.
            var diminta = terpesan
                .Where(x => x.OrderedStatus != LabOrderedProcedureStatus.Cancelled)
                .Select(x => x.ProcedureId)
                .ToHashSet();

            if (procedureIds.Any(id => !diminta.Contains(id)))
            {
                throw new LabSpecimenValidationException(
                    "Pemeriksaan ini tidak ada pada daftar yang dipesan untuk pasien ini.");
            }

            // VAL-69. Dinilai dari baris pemeriksaan yang benar-benar hidup, bukan dari penanda
            // Fulfilled pada baris terpesan.
            //
            // Sebabnya satu perkara nyata: membatalkan wadah TIDAK membatalkan pemeriksaan di
            // dalamnya, dan penanda pada baris terpesan akan tetap Fulfilled sesudahnya. Bila
            // penjagaan ini membaca penanda itu, pemeriksaan yang wadahnya dibatalkan tidak akan
            // pernah bisa diwadahi ulang — petugas terkunci tanpa jalan keluar, atas permintaan
            // yang masih sah. Membaca keadaan yang sebenarnya membuat penjagaan ini pulih
            // sendiri: wadah dibatalkan, pemeriksaannya ikut tidak dihitung, dan perencanaan
            // ulang terbuka lagi.
            var sudahBerwadah = await _dbContext.LabExaminations
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.LabOrderId == order.Id &&
                        procedureIds.Contains(x.ProcedureId) &&
                        !x.IsDelete &&
                        x.ExaminationStatus != LabExaminationStatus.Cancelled &&
                        x.Specimen != null &&
                        !x.Specimen.IsDelete &&
                        x.Specimen.SpecimenStatus != LabSpecimenStatus.Cancelled,
                    cancellationToken);

            if (sudahBerwadah)
            {
                throw new LabSpecimenConflictException(
                    "Pemeriksaan ini sudah masuk wadah lain.");
            }
        }

        /// <summary>
        /// <c>AC-91</c> — menandai permintaan yang baru saja memperoleh wadahnya, dan menautkannya
        /// ke baris pemeriksaan yang mengerjakannya.
        ///
        /// Tautan inilah yang membuat pertanyaan "mana yang masih menunggu wadah" dapat dijawab
        /// tanpa menebak. Tanpanya, permintaan dan pemeriksaan hanya bertemu lewat kesamaan
        /// <c>ProcedureId</c> — cukup untuk menghitung, tidak cukup untuk menunjuk.
        ///
        /// Pesanan tanpa baris terpesan tidak menghasilkan apa pun di sini, sejalan dengan
        /// <see cref="EnsureOrderedProcedureGuardAsync"/>.
        /// </summary>
        private async Task MarkOrderedProceduresFulfilledAsync(
            LabOrder order,
            IReadOnlyList<LabExamination> dibuat,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            if (dibuat.Count == 0) return;

            var procedureIds = dibuat.Select(x => x.ProcedureId).ToList();

            var terpesan = await _dbContext.LabOrderedProcedures
                .Where(x =>
                    x.LabOrderId == order.Id &&
                    !x.IsDelete &&
                    x.OrderedStatus != LabOrderedProcedureStatus.Cancelled &&
                    procedureIds.Contains(x.ProcedureId))
                .ToListAsync(cancellationToken);

            if (terpesan.Count == 0) return;

            foreach (var baris in terpesan)
            {
                var examination = dibuat.First(x => x.ProcedureId == baris.ProcedureId);

                baris.OrderedStatus = LabOrderedProcedureStatus.Fulfilled;
                baris.FulfilledExaminationId = examination.Id;
                baris.UpdateDateTime = now;
                baris.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Mengembalikan permintaan menjadi <c>Ordered</c> ketika wadah yang memenuhinya
        /// dibatalkan.
        ///
        /// <b>Kenapa ini ada.</b> Membatalkan wadah tidak membatalkan permintaan dokternya —
        /// bahannya yang gugur, bukan yang diminta. Tanpa pengembalian ini permintaan itu akan
        /// terbaca <c>Fulfilled</c> selamanya sambil menunjuk pemeriksaan pada wadah yang sudah
        /// dibatalkan, dan daftar "menunggu wadah" pada <c>AC-91</c> akan diam-diam kehilangan
        /// satu baris yang sebenarnya masih menunggu.
        ///
        /// Hanya baris yang benar-benar menunjuk pemeriksaan wadah ini yang dikembalikan.
        /// </summary>
        private async Task ReleaseOrderedProceduresAsync(
            LabOrder order,
            LabSpecimen specimen,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var examinationIds = await _dbContext.LabExaminations
                .AsNoTracking()
                .Where(x => x.SpecimenId == specimen.Id && !x.IsDelete)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            if (examinationIds.Count == 0) return;

            var terpesan = await _dbContext.LabOrderedProcedures
                .Where(x =>
                    x.LabOrderId == order.Id &&
                    !x.IsDelete &&
                    x.FulfilledExaminationId != null &&
                    examinationIds.Contains(x.FulfilledExaminationId.Value))
                .ToListAsync(cancellationToken);

            foreach (var baris in terpesan)
            {
                baris.OrderedStatus = LabOrderedProcedureStatus.Ordered;
                baris.FulfilledExaminationId = null;
                baris.UpdateDateTime = now;
                baris.UpdateBy = actorUserId;
            }
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
            //
            // Confirmed ikut di sini sejak BE-LAB-31, dan itu bukan kerapian melainkan keharusan:
            // LAB-STATE-v1 r3 bagian 1a menuliskan Confirmed -> Accepted sebagai turunan otomatis
            // sistem, persis seperti Requested -> Accepted. Tanpa baris ini pesanan yang sudah
            // dikonfirmasi akan berhenti selamanya di Confirmed — StartProcessAsync hanya
            // menerima Accepted — sehingga mengonfirmasi pesanan justru membuatnya tidak dapat
            // dikerjakan. Penambahannya aman: nol pesanan berstatus Confirmed sebelum endpoint
            // konfirmasi ada, sehingga tidak ada perilaku lama yang berubah.
            if (order.OrderStatus is LabOrderStatus.Draft
                or LabOrderStatus.Requested
                or LabOrderStatus.Confirmed)
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

            // Jenis bahan dan keterangan Lainnya-nya ikut pindah ke wadah pengganti: bahannya
            // diambil ulang, bukan berganti jenis, dan permintaan pengambilan ulang memang tidak
            // punya ruas untuk menyatakannya. Volumenya TIDAK ikut — bahan penggantinya belum
            // diambil, sehingga berapa banyak yang akan terkumpul belum diketahui.
            var replacement = await CreateSpecimenAsync(
                order,
                nextSequence + 1,
                specimen.SpecimenDescription,
                new LabSpecimenMaterial(
                    specimen.SpecimenType,
                    specimen.SpecimenTypeOtherNote,
                    VolumeAmount: null,
                    VolumeUnit: null),
                // Waktu penerimaan fisik tidak ikut diwariskan, dengan alasan yang sama seperti
                // volume: bahan penggantinya belum diambil, apalagi sampai di meja penerimaan.
                physicallyReceivedAt: null,
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

            // AC-91. Wadahnya gugur, permintaannya tidak — baris terpesan yang menunjuk
            // pemeriksaan wadah ini dikembalikan menjadi menunggu supaya dapat diwadahi ulang.
            await ReleaseOrderedProceduresAsync(order, specimen, actorUserId, now, cancellationToken);

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
            // AC-67. Rekap penerimaan menempatkan wadah menurut WAKTU NYATA-nya, bukan waktu
            // datanya masuk ke sistem. Wadah yang tiba Senin 21.10 dan baru diregistrasi Selasa
            // 08.05 karena itu terhitung pada hari Senin.
            //
            // CreateDateTime dipakai sebagai cadangan, bukan sebagai pilihan kedua yang setara:
            // wadah lama dan wadah yang waktu kedatangannya memang tidak dicatat tetap harus
            // muncul pada rekap, dan untuk baris-baris itu perilakunya sama persis seperti
            // sebelum LAB-DEC-042.
            var source = _dbContext.LabSpecimens
                .AsNoTracking()
                .Where(x => !x.IsDelete &&
                            (x.PhysicallyReceivedAt ?? x.CreateDateTime) >= startDate &&
                            (x.PhysicallyReceivedAt ?? x.CreateDateTime) <= endDate);

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

        /// <summary>
        /// Daftar penerimaan <b>lintas pesanan</b> (<c>LAB-API-v1</c> <c>r17</c>,
        /// <c>FR-11.5</c>, <c>AC-67</c>).
        ///
        /// <b>Rentangnya disaring pada waktu kedatangan SEBENARNYA</b> —
        /// <c>PhysicallyReceivedAt ?? CreateDateTime</c> — persis seperti
        /// <see cref="GetSummaryAsync"/>, dan itulah seluruh isi method ini.
        ///
        /// Wadah yang tiba <b>Senin 21.10</b> dan baru diregistrasi <b>Selasa 08.05</b> wajib
        /// muncul pada rentang hari <b>Senin</b>. Bila penyaringnya keliru memakai
        /// <c>CreateDateTime</c> saja, endpointnya tetap berjalan, tetap mengembalikan baris,
        /// dan tetap terlihat benar — hanya tanggalnya yang salah. Kegagalan yang tidak
        /// menimbulkan galat tidak akan ditemukan siapa pun sampai ada yang membandingkannya
        /// dengan kertas (<c>LAB-DEC-042</c>).
        ///
        /// <c>CreateDateTime</c> ikut dikembalikan supaya <b>selisih</b> kedua waktu itu dapat
        /// ditampilkan kepala instalasi.
        /// </summary>
        public async Task<PagedResult<LabSpecimenListResponse>> GetListAsync(
            LabSpecimenPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);

            var source = _dbContext.LabSpecimens
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            // Inilah baris yang menentukan. Lihat catatan pada ringkasan method.
            if (query.StartDate.HasValue)
            {
                var awal = query.StartDate.Value;
                source = source.Where(x => (x.PhysicallyReceivedAt ?? x.CreateDateTime) >= awal);
            }

            if (query.EndDate.HasValue)
            {
                var akhir = query.EndDate.Value;
                source = source.Where(x => (x.PhysicallyReceivedAt ?? x.CreateDateTime) <= akhir);
            }

            if (query.SpecimenStatus.HasValue)
                source = source.Where(x => x.SpecimenStatus == query.SpecimenStatus.Value);

            var search = (query.Search ?? string.Empty).Trim();

            if (!string.IsNullOrEmpty(search))
            {
                var pattern = $"%{search}%";

                source = source.Where(x =>
                    EF.Functions.ILike(x.SpecimenBarcode, pattern) ||
                    (x.LabOrder != null && EF.Functions.ILike(x.LabOrder.OrderNumber, pattern)));
            }

            var totalData = await source.CountAsync(cancellationToken);

            // Diurutkan menurut waktu kedatangan sebenarnya, terbaru lebih dulu — sama dengan
            // yang dipakai menyaring, supaya urutan dan penyaringnya tidak bercerita berbeda.
            var items = await source
                .OrderByDescending(x => x.PhysicallyReceivedAt ?? x.CreateDateTime)
                .ThenByDescending(x => x.SpecimenSequence)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LabSpecimenListResponse
                {
                    Id = x.Id,
                    LabOrderId = x.LabOrderId,
                    OrderNumber = x.LabOrder != null ? x.LabOrder.OrderNumber : string.Empty,
                    // Nama pasien diterjemahkan DI DALAM proyeksi yang sama, bukan lewat
                    // pencarian per baris sesudahnya — pola yang sama dengan LabMonitoringService,
                    // supaya daftar 25 baris tidak berubah menjadi 51 perjalanan ke database.
                    PatientName = x.LabOrder == null || x.LabOrder.Encounter == null
                        ? null
                        : _dbContext.MstPatients
                            .Where(pt => pt.Id == x.LabOrder.Encounter.PatientId)
                            .Select(pt => pt.FullName)
                            .FirstOrDefault(),
                    MedicalRecordNumber = x.LabOrder == null || x.LabOrder.Encounter == null
                        ? null
                        : _dbContext.MstPatients
                            .Where(pt => pt.Id == x.LabOrder.Encounter.PatientId)
                            .Select(pt => pt.MedicalRecordNumber)
                            .FirstOrDefault(),
                    SpecimenBarcode = x.SpecimenBarcode,
                    SpecimenSequence = x.SpecimenSequence,
                    SpecimenDescription = x.SpecimenDescription,
                    SpecimenTypeId = x.SpecimenTypeId,
                    SpecimenTypeName = x.SpecimenType != null ? x.SpecimenType.SpecimenTypeName : null,
                    SpecimenTypeOtherNote = x.SpecimenTypeOtherNote,
                    VolumeAmount = x.VolumeAmount,
                    VolumeUnitId = x.VolumeUnitId,
                    VolumeUnitSymbol = x.VolumeUnit != null ? x.VolumeUnit.MeasurementSymbol : null,
                    SpecimenStatus = x.SpecimenStatus.ToString(),
                    CollectedAt = x.CollectedAt,
                    ReceivedAt = x.ReceivedAt,
                    PhysicallyReceivedAt = x.PhysicallyReceivedAt,
                    CreateDateTime = x.CreateDateTime,
                    DecidedAt = x.DecidedAt,
                    RejectionReasonCode = x.RejectionReasonCode,
                    RejectionNote = x.RejectionNote,
                    SupersededSpecimenId = x.SupersededSpecimenId,
                    RecollectionCause = x.RecollectionCause != null ? x.RecollectionCause.ToString() : null,
                    Version = x.Version
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<LabSpecimenListResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
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
                    SpecimenTypeId = x.SpecimenTypeId,
                    SpecimenTypeName = x.SpecimenType != null ? x.SpecimenType.SpecimenTypeName : null,
                    SpecimenTypeOtherNote = x.SpecimenTypeOtherNote,
                    VolumeAmount = x.VolumeAmount,
                    VolumeUnitId = x.VolumeUnitId,
                    VolumeUnitSymbol = x.VolumeUnit != null ? x.VolumeUnit.MeasurementSymbol : null,
                    SpecimenStatus = x.SpecimenStatus.ToString(),
                    CollectedAt = x.CollectedAt,
                    ReceivedAt = x.ReceivedAt,
                    PhysicallyReceivedAt = x.PhysicallyReceivedAt,
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

        /// <summary>
        /// Bahan yang dibawa sebuah wadah: jenisnya, keterangan <c>Lainnya</c>-nya, volumenya,
        /// dan satuan volumenya. Dikumpulkan menjadi satu supaya jalur perencanaan dan jalur
        /// pengambilan ulang menyimpan hal yang sama persis.
        /// </summary>
        private sealed record LabSpecimenMaterial(
            LabSpecimenType? Type,
            string? OtherNote,
            decimal? VolumeAmount,
            MstMeasurement? VolumeUnit);

        /// <summary>
        /// Menegakkan <c>VAL-51</c> sampai <c>VAL-57</c> atas jenis dan volume wadah baru.
        ///
        /// Urutan pemeriksaannya disengaja. Jenis diperiksa lebih dulu karena keterangan
        /// <c>Lainnya</c> hanya bermakna setelah diketahui jenisnya, dan volume diperiksa
        /// terakhir karena ia yang paling sering dikosongkan petugas.
        ///
        /// <b>Tidak ada satu baris pun di sini yang membandingkan volume terhadap batas.</b>
        /// <c>RULE-021</c> menyatakan tidak ada batas minimum maupun maksimum dan
        /// <c>LAB-DEC-041</c> menerimanya apa adanya. Keputusan yang bentuknya <i>ketiadaan
        /// aturan</i> paling mudah dilanggar tanpa sengaja — seorang implementer yang bermaksud
        /// baik menambahkan peringatan "volume terlalu sedikit", dan aturannya hilang tanpa
        /// seorang pun memutuskannya. Yang menyatakan bahan tidak cukup adalah petugas, lewat
        /// penetapan kelayakan (<c>AC-63</c>).
        /// </summary>
        private async Task<LabSpecimenMaterial> ResolveSpecimenMaterialAsync(
            PlanLabSpecimenRequest request,
            CancellationToken cancellationToken)
        {
            var otherNote = string.IsNullOrWhiteSpace(request.SpecimenTypeOtherNote)
                ? null
                : request.SpecimenTypeOtherNote.Trim();

            var specimenTypeId = request.SpecimenTypeId.GetValueOrDefault();

            if (specimenTypeId == Guid.Empty)
            {
                // VAL-52. Keterangan yang dikirim tanpa satu pun jenis terpilih adalah upaya
                // menamai jenisnya sebagai teks bebas — persis yang dicegah LAB-DEC-040.
                if (otherNote != null)
                {
                    throw new LabSpecimenValidationException(
                        "Jenis specimen harus dipilih dari daftar. Bila jenisnya belum ada, " +
                        "pilih Lainnya lalu tuliskan keterangannya.");
                }

                // VAL-51.
                throw new LabSpecimenValidationException("Pilih jenis specimen terlebih dahulu.");
            }

            // Dimuat terlacak, bukan AsNoTracking. Alasannya ada pada CreateSpecimenAsync:
            // entity ini ditempelkan sebagai navigation pada wadah yang sedang dibuat.
            var specimenType = await _dbContext.LabSpecimenTypes
                .FirstOrDefaultAsync(x => x.Id == specimenTypeId && !x.IsDelete, cancellationToken);

            // VAL-52. Penunjuk yang tidak ada pada daftar diperlakukan sama seperti teks bebas:
            // keduanya sama-sama bukan pilihan dari daftar terkendali.
            if (specimenType == null)
            {
                throw new LabSpecimenValidationException(
                    "Jenis specimen harus dipilih dari daftar. Bila jenisnya belum ada, " +
                    "pilih Lainnya lalu tuliskan keterangannya.");
            }

            // VAL-55.
            if (!specimenType.IsActive)
            {
                throw new LabSpecimenValidationException(
                    "Jenis specimen ini sudah tidak dipakai lagi. Pilih jenis lain.");
            }

            if (specimenType.IsOtherBucket)
            {
                // VAL-53.
                if (otherNote == null)
                {
                    throw new LabSpecimenValidationException(
                        "Tuliskan jenis specimennya pada kolom keterangan.");
                }
            }
            else if (otherNote != null)
            {
                // VAL-54.
                throw new LabSpecimenValidationException(
                    "Keterangan jenis hanya diisi bila jenisnya Lainnya.");
            }

            var volumeUnitId = request.VolumeUnitId.GetValueOrDefault();

            // VAL-56. Angka tanpa satuan tidak berarti apa-apa: 3 bisa berarti 3 mL atau 3 slide.
            if (request.VolumeAmount.HasValue && volumeUnitId == Guid.Empty)
                throw new LabSpecimenValidationException("Pilih satuan volumenya.");

            MstMeasurement? volumeUnit = null;

            if (volumeUnitId != Guid.Empty)
            {
                // VAL-57. Penyaring IsForLaboratory-lah yang memisahkan satuan laboratorium dari
                // satuan obat, berat badan, dan satuan umum pada tabel yang sama.
                volumeUnit = await _dbContext.Set<MstMeasurement>()
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id == volumeUnitId &&
                            x.IsForLaboratory &&
                            x.IsActive &&
                            !x.IsDelete,
                        cancellationToken);

                if (volumeUnit == null)
                {
                    throw new LabSpecimenValidationException(
                        "Satuan ini tidak dipakai laboratorium. Pilih dari daftar satuan yang tersedia.");
                }
            }

            return new LabSpecimenMaterial(specimenType, otherNote, request.VolumeAmount, volumeUnit);
        }

        /// <summary>
        /// Menegakkan <c>VAL-58</c> dan <c>VAL-59</c> atas waktu penerimaan fisik.
        ///
        /// <b>Batas yang perlu diketahui pembaca berikutnya.</b> <c>VAL-59</c> membandingkan
        /// waktu penerimaan fisik terhadap <b>waktu pengambilan</b> specimen. Pada kontrak
        /// <c>LAB-API-v1</c> <c>r7</c>, satu-satunya permintaan yang membawa waktu penerimaan
        /// fisik adalah <see cref="PlanLabSpecimenRequest"/> — dan pada saat wadah direncanakan,
        /// <c>CollectedAt</c> masih kosong karena diisi server nanti pada tindakan pengambilan.
        /// Pembandingnya karena itu belum ada, dan <c>VAL-59</c> belum dapat ditegakkan.
        ///
        /// Menegakkannya pada tindakan pengambilan justru salah: <c>CollectedAt</c> di sana
        /// adalah waktu petugas menekan tombol di laboratorium, yang pada wadah rujukan luar
        /// hampir selalu <b>lebih akhir</b> daripada waktu kedatangannya. Penjagaan seperti itu
        /// akan menolak persis skenario yang dicontohkan <c>BR-37</c> sendiri — wadah tiba Senin
        /// 21.10, diregistrasi Selasa 08.05.
        ///
        /// Parameternya tetap disediakan supaya aturannya berdiri utuh begitu kontrak memberi
        /// jalan bagi waktu pengambilan yang dinyatakan petugas.
        /// </summary>
        private static DateTime? ResolvePhysicalReceipt(
            DateTime? physicallyReceivedAt,
            DateTime? collectedAt,
            DateTime now)
        {
            if (!physicallyReceivedAt.HasValue)
                return null;

            var nilai = physicallyReceivedAt.Value;

            // VAL-58. Wadah tidak dapat tiba pada waktu yang belum terjadi.
            if (nilai > now)
            {
                throw new LabSpecimenValidationException(
                    "Waktu penerimaan tidak boleh melewati waktu sekarang.");
            }

            // VAL-59.
            if (collectedAt.HasValue && nilai < collectedAt.Value)
            {
                throw new LabSpecimenValidationException(
                    "Waktu penerimaan tidak boleh lebih awal daripada waktu pengambilan.");
            }

            return nilai;
        }

        /// <summary>
        /// Menyusun catatan jejak audit yang memuat <b>kedua waktu berdampingan</b> beserta
        /// selisihnya (<c>LAB-DEC-042</c> butir 5, <c>AC-67</c>).
        ///
        /// Selisih itulah yang membuat keterlambatan pencatatan dapat ditelusuri tanpa menuduh
        /// siapa pun: selisih sebelas jam pada sampel yang datang pukul 21.00 adalah jam
        /// operasional, sedangkan selisih yang sama pada sampel yang datang pukul 10.00 adalah
        /// pertanyaan yang pantas diajukan.
        /// </summary>
        private static string ComposePhysicalReceiptNote(DateTime physicallyReceivedAt, DateTime systemTime)
        {
            var selisihMenit = (int)Math.Floor((systemTime - physicallyReceivedAt).TotalMinutes);

            return string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "Waktu penerimaan fisik {0:yyyy-MM-dd HH:mm} UTC; tercatat sistem {1:yyyy-MM-dd HH:mm} UTC; selisih {2} menit.",
                physicallyReceivedAt,
                systemTime,
                selisihMenit);
        }

        private async Task<LabSpecimen> CreateSpecimenAsync(
            LabOrder order,
            int sequence,
            string? description,
            LabSpecimenMaterial material,
            DateTime? physicallyReceivedAt,
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
                SpecimenTypeOtherNote = material.OtherNote,
                VolumeAmount = material.VolumeAmount,
                PhysicallyReceivedAt = physicallyReceivedAt,
                SupersededSpecimenId = supersededSpecimenId,
                RecollectionCause = recollectionCause,
                RecollectionReason = recollectionReason,
                RecollectionAuthorizedByUserId = recollectionCause != null ? actorUserId : null,
                RecollectionAuthorizedAt = recollectionCause != null ? now : null,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            // Navigation disetel, bukan hanya foreign keynya, supaya respons tindakan ini dapat
            // menyebut nama jenis dan simbol satuannya tanpa satu query tambahan.
            //
            // Keduanya dimuat TERLACAK oleh ResolveSpecimenMaterialAsync justru karena assignment
            // ini: menempelkan entity yang tidak terlacak pada entity yang sedang ditambahkan
            // membuat EF ikut menandainya Added, dan baris data induk yang sudah ada akan
            // disisipkan ulang lalu ditolak index unik.
            specimen.SpecimenType = material.Type;
            specimen.VolumeUnit = material.VolumeUnit;

            _dbContext.LabSpecimens.Add(specimen);

            // Kedua waktu dicatat berdampingan pada jejak audit beserta selisihnya. Baris
            // riwayat inilah yang membuat keterlambatan pencatatan dapat ditelusuri kemudian,
            // dan OccurredAt-nya adalah waktu sistem yang menjadi pembanding.
            var reasonNote = physicallyReceivedAt.HasValue
                ? ComposePhysicalReceiptNote(physicallyReceivedAt.Value, now)
                : recollectionReason;

            AppendHistory(
                order,
                specimen,
                LabTransitionScope.LabSpecimen,
                supersededSpecimenId == null ? "Specimen.Plan" : "Specimen.PlanRecollection",
                fromStatus: null,
                LabSpecimenStatus.Planned.ToString(),
                reasonCode: null,
                reasonNote,
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
            // Jenis dan satuan ikut dimuat supaya respons setiap tindakan menyebut nama jenis dan
            // simbol satuannya, bukan hanya penunjuknya. Keduanya baris data induk yang kecil.
            var specimen = await _dbContext.LabSpecimens
                .Include(x => x.LabOrder)
                .Include(x => x.SpecimenType)
                .Include(x => x.VolumeUnit)
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
