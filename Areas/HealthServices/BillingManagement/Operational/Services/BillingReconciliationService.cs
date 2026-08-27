using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services
{
    /// <summary>
    /// Rekonsiliasi dan pemulihan Billing — <c>RJ-BIL-BE-007</c>, melaksanakan
    /// <c>RJ-BIL-GATE-DEC-008</c> dan <c>RJ-BIL-CAP-017</c>.
    ///
    /// Yang dikerjakan service ini ada empat.
    ///
    /// Pertama, <b>menemukan</b>. Pemindaian membandingkan efek pemrosesan yang tercatat dengan
    /// keadaan Billing, lalu membuka case untuk setiap ketidaksesuaian yang tidak dapat
    /// diselesaikan secara deterministik.
    ///
    /// Kedua, <b>menampilkan</b>. Case yang terbuka punya pemilik, prioritas, umur, SLA, dan
    /// tindakan berikutnya, sehingga tidak ada kegagalan yang hilang begitu saja dari pandangan.
    ///
    /// Ketiga, <b>menahan</b>. Folio tidak boleh ditutup selama masih ada hal yang menahannya.
    /// Service ini yang menjawab boleh atau tidak, beserta alasannya satu per satu.
    ///
    /// Keempat, <b>menjawab</b>. Modul klinis yang kehilangan jawaban atas pengirimannya dapat
    /// menanyakan status kanonik berdasarkan identitas sumber, alih-alih mengirim ulang secara
    /// buta. Inilah yang mencegah kehilangan jawaban berubah menjadi tagihan ganda.
    ///
    /// Yang <b>tidak</b> dikerjakan service ini adalah menentukan akibat finansial. Tidak ada
    /// satu pun method di sini yang membayar, membatalkan, mengembalikan dana, atau menghapus
    /// tagihan. Rekonsiliasi berhenti pada temuan; akibat finansialnya tunduk pada
    /// <c>RJ-BIL-GATE-DEC-006</c> melalui jalur persetujuan <c>RJ-BIL-BE-006</c>.
    /// </summary>
    public class BillingReconciliationService
    {
        private const string CaseNumberPrefix = "RC";

        private readonly ApplicationDbContext _dbContext;

        public BillingReconciliationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // =================================================================
        // Pemindaian
        // =================================================================

        /// <summary>
        /// Memindai efek pemrosesan dan membuka case untuk yang bermasalah.
        ///
        /// Pemindaian ini <b>idempoten</b>. Masalah yang sama tidak pernah melahirkan case
        /// kedua, karena identitas case adalah kombinasi jenis, konteks sumber, identitas fakta,
        /// versi fakta, dan jenis efek — dijaga unique index di database, bukan hanya oleh
        /// pemeriksaan di memori.
        /// </summary>
        public async Task<ReconciliationScanResponse> ScanAsync(
            Guid? encounterId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var policies = await LoadPoliciesAsync(cancellationToken);

            var effectQuery = _dbContext.Set<BilProcessingEffect>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (encounterId.HasValue)
            {
                var folioIds = await _dbContext.Set<BilFolio>()
                    .AsNoTracking()
                    .Where(x => x.EncounterId == encounterId.Value && !x.IsDelete)
                    .Select(x => x.Id)
                    .ToListAsync(cancellationToken);

                effectQuery = effectQuery.Where(x => x.FolioId.HasValue && folioIds.Contains(x.FolioId.Value));
            }

            var effects = await effectQuery.ToListAsync(cancellationToken);

            var response = new ReconciliationScanResponse
            {
                ScannedAt = now,
                EffectsExamined = effects.Count
            };

            foreach (var effect in effects)
            {
                var caseType = MapOutcomeToCaseType(effect.Outcome);
                if (caseType == null)
                    continue;

                var encounterForCase = await ResolveEncounterIdAsync(effect, cancellationToken);
                if (encounterForCase == Guid.Empty)
                    continue;

                var opened = await OpenOrReuseCaseAsync(
                    caseType.Value,
                    effect,
                    encounterForCase,
                    policies,
                    now,
                    actorUserId,
                    cancellationToken);

                if (opened.Reused)
                {
                    response.CasesReused++;
                }
                else
                {
                    response.CasesOpened++;
                    response.OpenedCases.Add(MapCase(opened.Case, now));
                }
            }

            response.SlaBreachesMarked = await MarkSlaBreachesAsync(now, actorUserId, cancellationToken);

            return response;
        }

        /// <summary>
        /// Menentukan jenis case dari hasil pemrosesan. Mengembalikan <c>null</c> untuk hasil
        /// yang memang tidak bermasalah, sehingga keberhasilan tidak pernah membuka case.
        /// </summary>
        private static BillingReconciliationCaseType? MapOutcomeToCaseType(BillingProcessingOutcome outcome) =>
            outcome switch
            {
                BillingProcessingOutcome.OutcomeUnknown => BillingReconciliationCaseType.OutcomeUnknown,
                BillingProcessingOutcome.PartialOutcome => BillingReconciliationCaseType.PartialComponentFailure,
                BillingProcessingOutcome.PermanentFailure => BillingReconciliationCaseType.PermanentFailure,

                // Peninggalan RJ-BIL-BE-002 yang tidak membedakan sebab kegagalan. Diperlakukan
                // sebagai kegagalan menetap karena tidak ada dasar untuk menyatakannya aman
                // diulang, dan menganggapnya sementara berarti mengulang sesuatu yang mungkin
                // sudah terlanjur diterapkan.
                BillingProcessingOutcome.FailedBeforeEffect => BillingReconciliationCaseType.PermanentFailure,

                _ => null
            };

        private sealed record OpenCaseOutcome(BilReconciliationCase Case, bool Reused);

        private async Task<OpenCaseOutcome> OpenOrReuseCaseAsync(
            BillingReconciliationCaseType caseType,
            BilProcessingEffect effect,
            Guid encounterId,
            IReadOnlyDictionary<BillingReconciliationCaseType, MstBillingReconciliationPolicy> policies,
            DateTime now,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var existing = await _dbContext.Set<BilReconciliationCase>()
                .FirstOrDefaultAsync(
                    x => x.CaseType == caseType &&
                         x.SourceContext == effect.SourceContext &&
                         x.MilestoneFactId == effect.MilestoneFactId &&
                         x.MilestoneFactVersion == effect.MilestoneFactVersion &&
                         x.EffectType == effect.EffectType &&
                         !x.IsDelete,
                    cancellationToken);

            if (existing != null)
                return new OpenCaseOutcome(existing, Reused: true);

            var policy = policies.TryGetValue(caseType, out var found) ? found : null;
            var impact = await ResolveImpactAmountAsync(effect, cancellationToken);

            var reconciliationCase = new BilReconciliationCase
            {
                Id = Guid.NewGuid(),
                CaseNumber = BuildCaseNumber(now),
                CaseType = caseType,
                SourceContext = effect.SourceContext,
                MilestoneFactId = effect.MilestoneFactId,
                MilestoneFactVersion = effect.MilestoneFactVersion,
                EffectType = effect.EffectType,
                ProcessingEffectId = effect.Id,
                EncounterId = encounterId,
                FolioId = effect.FolioId,
                ChargeLineId = effect.ChargeLineId,
                ImpactAmount = impact,
                ImpactDescription = BuildImpactDescription(caseType, impact),
                BlocksFolioClosure = IsMaterial(caseType, impact, policy),
                CaseStatus = BillingReconciliationCaseStatus.Open,
                Priority = policy?.DefaultPriority ?? BillingReconciliationPriority.Normal,
                DetectedAt = now,
                SlaDueAt = policy != null && policy.SlaMinutes > 0
                    ? now.AddMinutes(policy.SlaMinutes)
                    : null,
                AttemptCount = 0,
                NextAction = BuildNextAction(caseType),
                FailureReason = effect.ErrorMessage ?? effect.ErrorCode,
                CorrelationId = effect.CorrelationId,
                Version = 1,
                CreateBy = actorUserId,
                CreateDateTime = now
            };

            _dbContext.Set<BilReconciliationCase>().Add(reconciliationCase);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Pemindaian lain menang balapan dan unique index menolak case kedua. Yang benar
                // adalah memakai case miliknya, bukan memaksakan salinan untuk masalah yang sama.
                //
                // Pemeriksaannya dilakukan di dalam blok catch, bukan pada filter `when`, karena
                // C# tidak mengizinkan await pada filter exception. Bila ternyata tidak ada case
                // pemenang, kegagalannya bukan balapan dan exception aslinya dilempar kembali
                // apa adanya — menelannya akan menyembunyikan kesalahan yang sesungguhnya.
                _dbContext.Entry(reconciliationCase).State = EntityState.Detached;

                var winner = await _dbContext.Set<BilReconciliationCase>()
                    .FirstOrDefaultAsync(
                        x => x.CaseType == caseType &&
                             x.SourceContext == effect.SourceContext &&
                             x.MilestoneFactId == effect.MilestoneFactId &&
                             x.MilestoneFactVersion == effect.MilestoneFactVersion &&
                             x.EffectType == effect.EffectType &&
                             !x.IsDelete,
                        cancellationToken);

                if (winner == null)
                    throw;

                return new OpenCaseOutcome(winner, Reused: true);
            }

            return new OpenCaseOutcome(reconciliationCase, Reused: false);
        }

        /// <summary>
        /// Menandai case yang melampaui SLA.
        ///
        /// Yang dilakukan hanya menandai dan menaikkan prioritas. Tidak ada case yang
        /// diselesaikan, tidak ada tagihan yang dihapus, dan tidak ada persetujuan yang
        /// diberikan — sesuai <c>RJ-BIL-GATE-DEC-008</c>, pelampauan SLA hanya soal perhatian,
        /// bukan soal keputusan.
        /// </summary>
        private async Task<int> MarkSlaBreachesAsync(
            DateTime now,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var terlampaui = await _dbContext.Set<BilReconciliationCase>()
                .Where(x => !x.IsDelete &&
                            x.SlaBreachedAt == null &&
                            x.SlaDueAt != null &&
                            x.SlaDueAt < now &&
                            x.CaseStatus != BillingReconciliationCaseStatus.Resolved &&
                            x.CaseStatus != BillingReconciliationCaseStatus.AutoResolved)
                .ToListAsync(cancellationToken);

            if (terlampaui.Count == 0)
                return 0;

            foreach (var item in terlampaui)
            {
                item.SlaBreachedAt = now;
                item.CaseStatus = BillingReconciliationCaseStatus.Escalated;
                item.Priority = Escalate(item.Priority);
                item.Version++;
                item.UpdateBy = actorUserId;
                item.UpdateDateTime = now;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return terlampaui.Count;
        }

        private static BillingReconciliationPriority Escalate(BillingReconciliationPriority current) =>
            current switch
            {
                BillingReconciliationPriority.Low => BillingReconciliationPriority.Normal,
                BillingReconciliationPriority.Normal => BillingReconciliationPriority.High,
                _ => BillingReconciliationPriority.Critical
            };

        // =================================================================
        // Kepemilikan dan penyelesaian
        // =================================================================

        public async Task<BillingServiceResult<BillingReconciliationCaseResponse>> AssignAsync(
            Guid caseId,
            AssignReconciliationCaseRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (request.OwnerUserId == Guid.Empty)
            {
                return BillingServiceResult<BillingReconciliationCaseResponse>.Validation(
                    "BIL_RECON_OWNER_INVALID",
                    "Pemilik case wajib diisi.");
            }

            var reconciliationCase = await _dbContext.Set<BilReconciliationCase>()
                .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

            if (reconciliationCase == null)
            {
                return BillingServiceResult<BillingReconciliationCaseResponse>.NotFound(
                    "BIL_RECON_CASE_NOT_FOUND",
                    "Reconciliation case tidak ditemukan.");
            }

            if (IsClosed(reconciliationCase.CaseStatus))
            {
                return BillingServiceResult<BillingReconciliationCaseResponse>.Conflict(
                    "BIL_RECON_CASE_CLOSED",
                    "Case yang sudah selesai tidak dapat ditugaskan ulang.");
            }

            var now = DateTime.UtcNow;

            reconciliationCase.OwnerUserId = request.OwnerUserId;
            reconciliationCase.AssignedAt = now;
            reconciliationCase.CaseStatus = BillingReconciliationCaseStatus.InProgress;

            if (request.Priority.HasValue)
                reconciliationCase.Priority = request.Priority.Value;

            if (!string.IsNullOrWhiteSpace(request.NextAction))
                reconciliationCase.NextAction = request.NextAction.Trim();

            reconciliationCase.Version++;
            reconciliationCase.UpdateBy = actorUserId;
            reconciliationCase.UpdateDateTime = now;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return BillingServiceResult<BillingReconciliationCaseResponse>.Success(
                MapCase(reconciliationCase, now));
        }

        /// <summary>
        /// Menutup sebuah case.
        ///
        /// Penyelesaian di sini bersifat <b>administratif</b>: ia menyatakan bahwa masalahnya
        /// sudah ditelusuri dan diketahui hasilnya. Bila hasil penelusuran menuntut tindakan
        /// finansial — pembatalan, koreksi, pengembalian dana — jenis penyelesaiannya adalah
        /// <see cref="BillingReconciliationResolutionType.ManualFinancialAction"/>, dan tindakan
        /// finansialnya dikerjakan lewat jalur persetujuan <c>RJ-BIL-GATE-DEC-006</c>. Case yang
        /// ditutup tidak pernah dengan sendirinya memindahkan uang.
        /// </summary>
        public async Task<BillingServiceResult<BillingReconciliationCaseResponse>> ResolveAsync(
            Guid caseId,
            ResolveReconciliationCaseRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.ResolutionNote))
            {
                return BillingServiceResult<BillingReconciliationCaseResponse>.Validation(
                    "BIL_RECON_RESOLUTION_NOTE_REQUIRED",
                    "Catatan penyelesaian wajib diisi agar keputusan dapat ditelusuri kembali.");
            }

            var reconciliationCase = await _dbContext.Set<BilReconciliationCase>()
                .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

            if (reconciliationCase == null)
            {
                return BillingServiceResult<BillingReconciliationCaseResponse>.NotFound(
                    "BIL_RECON_CASE_NOT_FOUND",
                    "Reconciliation case tidak ditemukan.");
            }

            if (IsClosed(reconciliationCase.CaseStatus))
            {
                return BillingServiceResult<BillingReconciliationCaseResponse>.Conflict(
                    "BIL_RECON_CASE_CLOSED",
                    "Case ini sudah selesai.");
            }

            var now = DateTime.UtcNow;

            reconciliationCase.CaseStatus = BillingReconciliationCaseStatus.Resolved;
            reconciliationCase.ResolutionType = request.ResolutionType;
            reconciliationCase.ResolutionNote = request.ResolutionNote.Trim();
            reconciliationCase.ResolvedAt = now;
            reconciliationCase.ResolvedByUserId = actorUserId;
            reconciliationCase.BlocksFolioClosure = false;
            reconciliationCase.Version++;
            reconciliationCase.UpdateBy = actorUserId;
            reconciliationCase.UpdateDateTime = now;

            await SyncProcessingEffectAsync(reconciliationCase, now, actorUserId, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return BillingServiceResult<BillingReconciliationCaseResponse>.Success(
                MapCase(reconciliationCase, now));
        }

        /// <summary>
        /// Menyelaraskan hasil pemrosesan dengan penyelesaian case-nya, sehingga pencarian
        /// status kanonik berikutnya menjawab hal yang sama dengan case-nya.
        /// </summary>
        private async Task SyncProcessingEffectAsync(
            BilReconciliationCase reconciliationCase,
            DateTime now,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            if (!reconciliationCase.ProcessingEffectId.HasValue)
                return;

            var effect = await _dbContext.Set<BilProcessingEffect>()
                .FirstOrDefaultAsync(
                    x => x.Id == reconciliationCase.ProcessingEffectId.Value && !x.IsDelete,
                    cancellationToken);

            if (effect == null)
                return;

            effect.Outcome = BillingProcessingOutcome.Reconciled;
            effect.CompletedAt ??= now;
            effect.UpdateBy = actorUserId;
            effect.UpdateDateTime = now;
        }

        // =================================================================
        // Gerbang penutupan folio
        // =================================================================

        /// <summary>
        /// Menjawab apakah sebuah folio boleh ditutup, beserta seluruh alasan yang menahannya.
        ///
        /// Gerbang ini <b>tidak</b> menutup folio. Penutupannya sendiri adalah ranah
        /// <c>RJ-BIL-BE-006</c>; yang disediakan di sini adalah jawabannya, agar penutupan tidak
        /// pernah terjadi sementara masih ada uang yang belum jelas nasibnya.
        /// </summary>
        public async Task<BillingServiceResult<FolioClosureReadinessResponse>> EvaluateClosureReadinessAsync(
            Guid folioId,
            CancellationToken cancellationToken = default)
        {
            var folio = await _dbContext.Set<BilFolio>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == folioId && !x.IsDelete, cancellationToken);

            if (folio == null)
            {
                return BillingServiceResult<FolioClosureReadinessResponse>.NotFound(
                    "BIL_FOLIO_NOT_FOUND",
                    "Folio tidak ditemukan.");
            }

            var response = new FolioClosureReadinessResponse
            {
                FolioId = folio.Id,
                EncounterId = folio.EncounterId
            };

            var openCases = await _dbContext.Set<BilReconciliationCase>()
                .AsNoTracking()
                .Where(x => !x.IsDelete &&
                            x.FolioId == folioId &&
                            x.CaseStatus != BillingReconciliationCaseStatus.Resolved &&
                            x.CaseStatus != BillingReconciliationCaseStatus.AutoResolved)
                .ToListAsync(cancellationToken);

            foreach (var item in openCases.Where(x => x.BlocksFolioClosure))
            {
                response.Blockers.Add(new FolioClosureBlocker
                {
                    BlockerCode = $"RECONCILIATION_{item.CaseType}".ToUpperInvariant(),
                    Description = item.ImpactDescription,
                    ReconciliationCaseId = item.Id,
                    CaseNumber = item.CaseNumber,
                    ImpactAmount = item.ImpactAmount
                });
            }

            // Hasil yang belum pasti menahan penutupan walaupun case-nya belum sempat dibuka
            // pemindaian. Menutup folio sambil ada pengiriman yang hasilnya tidak diketahui
            // adalah cara paling langsung kehilangan tagihan tanpa jejak.
            var unknownOutcomes = await _dbContext.Set<BilProcessingEffect>()
                .AsNoTracking()
                .Where(x => !x.IsDelete &&
                            x.FolioId == folioId &&
                            (x.Outcome == BillingProcessingOutcome.OutcomeUnknown ||
                             x.Outcome == BillingProcessingOutcome.PendingReconciliation))
                .CountAsync(cancellationToken);

            if (unknownOutcomes > 0)
            {
                response.Blockers.Add(new FolioClosureBlocker
                {
                    BlockerCode = "PROCESSING_OUTCOME_UNKNOWN",
                    Description = $"Terdapat {unknownOutcomes} pemrosesan yang hasilnya belum dapat " +
                                  "dipastikan. Lakukan pencarian status kanonik lebih dulu."
                });
            }

            var pendingReview = await _dbContext.Set<BilChargeLine>()
                .AsNoTracking()
                .Where(x => !x.IsDelete &&
                            x.FolioId == folioId &&
                            x.CalculationStatus == BillingChargeCalculationStatus.PendingFinancialReview)
                .CountAsync(cancellationToken);

            if (pendingReview > 0)
            {
                response.Blockers.Add(new FolioClosureBlocker
                {
                    BlockerCode = "CHARGE_PENDING_FINANCIAL_REVIEW",
                    Description = $"Terdapat {pendingReview} baris tagihan yang masih menunggu telaah finansial."
                });
            }

            response.CanClose = response.Blockers.Count == 0;

            return BillingServiceResult<FolioClosureReadinessResponse>.Success(response);
        }

        // =================================================================
        // Pencarian status kanonik
        // =================================================================

        /// <summary>
        /// Menjawab hasil pemrosesan sebuah fakta berdasarkan identitas sumbernya yang stabil.
        ///
        /// Inilah jalan keluar dari kehilangan jawaban. Modul klinis yang tidak menerima balasan
        /// tidak boleh menyimpulkan gagal maupun berhasil; ia menanyakan hasilnya ke sini, lalu
        /// baru memutuskan. <see cref="BillingProcessingStatusResponse.SafeToRetryWithSameKey"/>
        /// menyatakan secara eksplisit apakah pengulangan aman, sehingga pemanggilnya tidak perlu
        /// menebak.
        /// </summary>
        public async Task<BillingProcessingStatusResponse> GetProcessingStatusAsync(
            string sourceContext,
            Guid milestoneFactId,
            int milestoneFactVersion,
            string effectType,
            CancellationToken cancellationToken = default)
        {
            var normalizedContext = sourceContext?.Trim() ?? string.Empty;
            var normalizedEffect = effectType?.Trim() ?? string.Empty;

            var response = new BillingProcessingStatusResponse
            {
                SourceContext = normalizedContext,
                MilestoneFactId = milestoneFactId,
                MilestoneFactVersion = milestoneFactVersion,
                EffectType = normalizedEffect
            };

            var effect = await _dbContext.Set<BilProcessingEffect>()
                .AsNoTracking()
                .Where(x => !x.IsDelete &&
                            x.SourceContext == normalizedContext &&
                            x.MilestoneFactId == milestoneFactId &&
                            x.MilestoneFactVersion == milestoneFactVersion &&
                            x.EffectType == normalizedEffect)
                .OrderByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);

            if (effect == null)
            {
                response.Found = false;
                response.SafeToRetryWithSameKey = true;
                response.Guidance =
                    "Tidak ada jejak pemrosesan atas identitas ini. Pengiriman belum pernah sampai, " +
                    "sehingga aman dikirim ulang dengan kunci idempotensi yang sama.";
                return response;
            }

            response.Found = true;
            response.Outcome = effect.Outcome;
            response.OutcomeName = effect.Outcome.ToString();
            response.FolioId = effect.FolioId;
            response.ChargeLineId = effect.ChargeLineId;
            response.CalculationStatus = effect.CalculationStatus;
            response.AppliedFactVersion = effect.MilestoneFactVersion;
            response.ErrorCode = effect.ErrorCode;
            response.ErrorMessage = effect.ErrorMessage;
            response.OccurredAt = effect.OccurredAt;
            response.CompletedAt = effect.CompletedAt;

            var relatedCase = await _dbContext.Set<BilReconciliationCase>()
                .AsNoTracking()
                .Where(x => !x.IsDelete &&
                            x.SourceContext == normalizedContext &&
                            x.MilestoneFactId == milestoneFactId &&
                            x.MilestoneFactVersion == milestoneFactVersion &&
                            x.EffectType == normalizedEffect)
                .OrderByDescending(x => x.DetectedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (relatedCase != null)
            {
                response.ReconciliationCaseId = relatedCase.Id;
                response.ReconciliationCaseNumber = relatedCase.CaseNumber;
                response.ReconciliationCaseStatus = relatedCase.CaseStatus;
            }

            (response.SafeToRetryWithSameKey, response.Guidance) = BuildRetryGuidance(effect.Outcome);

            return response;
        }

        private static (bool SafeToRetry, string Guidance) BuildRetryGuidance(BillingProcessingOutcome outcome) =>
            outcome switch
            {
                BillingProcessingOutcome.Succeeded => (false,
                    "Fakta sudah diterapkan. Pengiriman ulang tidak diperlukan dan tidak akan " +
                    "membentuk tagihan kedua."),

                BillingProcessingOutcome.Reconciled => (false,
                    "Sudah direkonsiliasi dan hasilnya diketahui. Tidak perlu dikirim ulang."),

                BillingProcessingOutcome.PartialOutcome => (false,
                    "Sebagian komponen sudah diterapkan. Pengiriman ulang seluruh fakta akan " +
                    "menggandakan komponen yang sudah berhasil. Selesaikan melalui reconciliation case."),

                BillingProcessingOutcome.TransientFailure => (true,
                    "Gangguan sementara. Aman dicoba ulang dengan kunci idempotensi yang sama."),

                BillingProcessingOutcome.RejectedValidation => (false,
                    "Ditolak validasi dan bersifat final untuk versi ini. Perbaikan dilakukan " +
                    "dengan menerbitkan versi fakta baru, bukan dengan mengulang versi yang sama."),

                BillingProcessingOutcome.PermanentFailure => (false,
                    "Kegagalan menetap. Percobaan ulang otomatis dihentikan; tunggu penyelesaian " +
                    "reconciliation case."),

                BillingProcessingOutcome.OutcomeUnknown => (false,
                    "Hasil belum dapat dipastikan. Jangan dikirim ulang sebelum reconciliation " +
                    "case-nya selesai, karena pengiriman ulang atas hasil yang belum terverifikasi " +
                    "adalah penyebab paling umum tagihan ganda."),

                BillingProcessingOutcome.PendingReconciliation => (false,
                    "Sedang menunggu rekonsiliasi. Jangan dikirim ulang."),

                _ => (false,
                    "Pemrosesan masih berjalan. Tunggu hasilnya sebelum mengambil tindakan.")
            };

        // =================================================================
        // Laporan pemulihan
        // =================================================================

        public async Task<BillingRecoveryReportResponse> GetRecoveryReportAsync(
            Guid? encounterId,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var caseQuery = _dbContext.Set<BilReconciliationCase>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var effectQuery = _dbContext.Set<BilProcessingEffect>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (encounterId.HasValue)
            {
                caseQuery = caseQuery.Where(x => x.EncounterId == encounterId.Value);

                var folioIds = await _dbContext.Set<BilFolio>()
                    .AsNoTracking()
                    .Where(x => x.EncounterId == encounterId.Value && !x.IsDelete)
                    .Select(x => x.Id)
                    .ToListAsync(cancellationToken);

                effectQuery = effectQuery.Where(x => x.FolioId.HasValue && folioIds.Contains(x.FolioId.Value));
            }

            var cases = await caseQuery.ToListAsync(cancellationToken);
            var effects = await effectQuery.ToListAsync(cancellationToken);

            var unresolved = cases
                .Where(x => x.CaseStatus != BillingReconciliationCaseStatus.Resolved &&
                            x.CaseStatus != BillingReconciliationCaseStatus.AutoResolved)
                .ToList();

            return new BillingRecoveryReportResponse
            {
                GeneratedAt = now,
                EncounterId = encounterId,

                OutcomeCounts = effects
                    .GroupBy(x => x.Outcome)
                    .Select(g => new RecoveryOutcomeCount
                    {
                        Outcome = g.Key,
                        OutcomeName = g.Key.ToString(),
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Outcome)
                    .ToList(),

                CaseTypeCounts = cases
                    .GroupBy(x => x.CaseType)
                    .Select(g => new RecoveryCaseTypeCount
                    {
                        CaseType = g.Key,
                        CaseTypeName = g.Key.ToString(),
                        Count = g.Count(),
                        ImpactAmount = g.Sum(x => x.ImpactAmount)
                    })
                    .OrderBy(x => x.CaseType)
                    .ToList(),

                UnresolvedCaseCount = unresolved.Count,
                UnassignedCaseCount = unresolved.Count(x => x.OwnerUserId == null),
                SlaBreachedCaseCount = unresolved.Count(x => x.SlaBreachedAt != null),
                UnresolvedImpactAmount = unresolved.Sum(x => x.ImpactAmount),

                AffectedEncounterIds = unresolved.Select(x => x.EncounterId).Distinct().ToList(),
                AffectedFolioIds = unresolved
                    .Where(x => x.FolioId.HasValue)
                    .Select(x => x.FolioId!.Value)
                    .Distinct()
                    .ToList(),

                UnresolvedCases = unresolved
                    .OrderByDescending(x => x.Priority)
                    .ThenBy(x => x.DetectedAt)
                    .Select(x => MapCase(x, now))
                    .ToList()
            };
        }

        // =================================================================
        // Pembacaan
        // =================================================================

        public async Task<List<BillingReconciliationCaseResponse>> GetCasesAsync(
            Guid? encounterId,
            Guid? folioId,
            BillingReconciliationCaseStatus? status,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var query = _dbContext.Set<BilReconciliationCase>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (encounterId.HasValue)
                query = query.Where(x => x.EncounterId == encounterId.Value);

            if (folioId.HasValue)
                query = query.Where(x => x.FolioId == folioId.Value);

            if (status.HasValue)
                query = query.Where(x => x.CaseStatus == status.Value);

            var cases = await query
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.DetectedAt)
                .ToListAsync(cancellationToken);

            return cases.Select(x => MapCase(x, now)).ToList();
        }

        public async Task<BillingReconciliationCaseResponse?> GetCaseByIdAsync(
            Guid caseId,
            CancellationToken cancellationToken = default)
        {
            var reconciliationCase = await _dbContext.Set<BilReconciliationCase>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

            return reconciliationCase == null ? null : MapCase(reconciliationCase, DateTime.UtcNow);
        }

        // =================================================================
        // Pembantu
        // =================================================================

        private async Task<IReadOnlyDictionary<BillingReconciliationCaseType, MstBillingReconciliationPolicy>>
            LoadPoliciesAsync(CancellationToken cancellationToken)
        {
            var policies = await _dbContext.Set<MstBillingReconciliationPolicy>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .ToListAsync(cancellationToken);

            return policies
                .GroupBy(x => x.CaseType)
                .ToDictionary(g => g.Key, g => g.First());
        }

        /// <summary>
        /// Menentukan apakah dampak sebuah case cukup material untuk menahan penutupan folio.
        ///
        /// Ketika kebijakannya belum diisi, jawabannya adalah <b>menahan</b>. Ketiadaan
        /// kebijakan bukan izin untuk melewatkan uang yang nasibnya belum jelas.
        /// </summary>
        private static bool IsMaterial(
            BillingReconciliationCaseType caseType,
            decimal impact,
            MstBillingReconciliationPolicy? policy)
        {
            if (policy == null)
                return true;

            return impact > policy.MaterialityThresholdAmount;
        }

        private async Task<decimal> ResolveImpactAmountAsync(
            BilProcessingEffect effect,
            CancellationToken cancellationToken)
        {
            if (!effect.ChargeLineId.HasValue)
                return 0m;

            var amount = await _dbContext.Set<BilChargeLine>()
                .AsNoTracking()
                .Where(x => x.Id == effect.ChargeLineId.Value && !x.IsDelete)
                .Select(x => (decimal?)x.GrossAmount)
                .FirstOrDefaultAsync(cancellationToken);

            return amount ?? 0m;
        }

        private async Task<Guid> ResolveEncounterIdAsync(
            BilProcessingEffect effect,
            CancellationToken cancellationToken)
        {
            if (!effect.FolioId.HasValue)
                return Guid.Empty;

            return await _dbContext.Set<BilFolio>()
                .AsNoTracking()
                .Where(x => x.Id == effect.FolioId.Value && !x.IsDelete)
                .Select(x => x.EncounterId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static bool IsClosed(BillingReconciliationCaseStatus status) =>
            status is BillingReconciliationCaseStatus.Resolved
                or BillingReconciliationCaseStatus.AutoResolved;

        private static string BuildCaseNumber(DateTime now) =>
            $"{CaseNumberPrefix}-{now:yyyyMMdd}-{Guid.NewGuid():N}"[..28].ToUpperInvariant();

        private static string BuildImpactDescription(
            BillingReconciliationCaseType caseType,
            decimal impact) =>
            caseType switch
            {
                BillingReconciliationCaseType.OutcomeUnknown =>
                    $"Hasil pemrosesan belum dapat dipastikan; nilai yang dipertaruhkan {impact:N2}.",
                BillingReconciliationCaseType.PartialComponentFailure =>
                    $"Sebagian komponen gagal diterapkan; nilai yang dipertaruhkan {impact:N2}.",
                BillingReconciliationCaseType.PermanentFailure =>
                    $"Pemrosesan gagal menetap; nilai yang dipertaruhkan {impact:N2}.",
                _ => $"Ketidaksesuaian {caseType}; nilai yang dipertaruhkan {impact:N2}."
            };

        private static string BuildNextAction(BillingReconciliationCaseType caseType) =>
            caseType switch
            {
                BillingReconciliationCaseType.OutcomeUnknown =>
                    "Lakukan pencarian status kanonik memakai identitas sumber yang sama, lalu " +
                    "tentukan hasilnya sebelum mengulang pengiriman.",
                BillingReconciliationCaseType.PartialComponentFailure =>
                    "Periksa komponen yang gagal satu per satu. Komponen yang sudah diterapkan " +
                    "tidak boleh dihapus karena komponen lain gagal.",
                BillingReconciliationCaseType.PermanentFailure =>
                    "Telaah sebab kegagalan. Bila sebabnya konfigurasi Billing, perbaiki lalu " +
                    "proses ulang versi fakta yang sama.",
                _ => "Telusuri ketidaksesuaian dan tentukan tindakan pada domain pemiliknya."
            };

        private static BillingReconciliationCaseResponse MapCase(BilReconciliationCase entity, DateTime now) =>
            new()
            {
                Id = entity.Id,
                CaseNumber = entity.CaseNumber,
                CaseType = entity.CaseType,
                CaseTypeName = entity.CaseType.ToString(),
                SourceContext = entity.SourceContext,
                MilestoneFactId = entity.MilestoneFactId,
                MilestoneFactVersion = entity.MilestoneFactVersion,
                EffectType = entity.EffectType,
                ProcessingEffectId = entity.ProcessingEffectId,
                EncounterId = entity.EncounterId,
                FolioId = entity.FolioId,
                ChargeLineId = entity.ChargeLineId,
                ImpactAmount = entity.ImpactAmount,
                ImpactDescription = entity.ImpactDescription,
                BlocksFolioClosure = entity.BlocksFolioClosure,
                CaseStatus = entity.CaseStatus,
                CaseStatusName = entity.CaseStatus.ToString(),
                Priority = entity.Priority,
                PriorityName = entity.Priority.ToString(),
                OwnerUserId = entity.OwnerUserId,
                AssignedAt = entity.AssignedAt,
                DetectedAt = entity.DetectedAt,
                SlaDueAt = entity.SlaDueAt,
                SlaBreachedAt = entity.SlaBreachedAt,
                AgeMinutes = (int)Math.Max(0, (now - entity.DetectedAt).TotalMinutes),
                SlaBreached = entity.SlaBreachedAt != null,
                AttemptCount = entity.AttemptCount,
                LastAttemptAt = entity.LastAttemptAt,
                NextAction = entity.NextAction,
                FailureReason = entity.FailureReason,
                ResolutionType = entity.ResolutionType,
                ResolutionTypeName = entity.ResolutionType?.ToString(),
                ResolutionNote = entity.ResolutionNote,
                ResolvedAt = entity.ResolvedAt,
                ResolvedByUserId = entity.ResolvedByUserId,
                CorrelationId = entity.CorrelationId,
                Version = entity.Version
            };
    }
}
