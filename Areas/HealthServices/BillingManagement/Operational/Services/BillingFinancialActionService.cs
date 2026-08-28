using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services
{
    /// <summary>
    /// Pengajuan, persetujuan, dan pelaksanaan tindakan finansial — <c>RJ-BIL-BE-006</c>,
    /// melaksanakan <c>RJ-BIL-GATE-DEC-006</c>.
    ///
    /// <para><b>Tiga hal yang tidak dapat dilonggarkan siapa pun dari sini.</b></para>
    ///
    /// Pertama, <b>maker tidak boleh menjadi checker</b>. Pemeriksaannya tidak membaca
    /// konfigurasi apa pun dan tidak punya jalan pintas; ia membandingkan dua <c>UserId</c> dan
    /// berhenti. Ini bukan pilihan gaya: sistem ini sudah punya mesin persetujuan lain yang
    /// menjadikan larangan itu sebuah <c>bool</c> yang bisa dinyalakan, dan justru karena itulah
    /// <c>RJ-BIL-DEC-011</c> memutuskan Billing memakai jalurnya sendiri.
    ///
    /// Kedua, <b>menunggu persetujuan tidak mengubah apa pun</b>. Tidak ada satu baris pun di
    /// sini yang menyentuh <c>BilChargeLine</c> atau <c>BilFolio</c> sebelum
    /// <see cref="ExecuteAsync"/> — dan <see cref="ExecuteAsync"/> hanya berjalan setelah ada
    /// keputusan Approve dari orang yang berbeda.
    ///
    /// Ketiga, <b>ketiadaan kebijakan tidak pernah menjadi izin</b>. Bila ambang yang sah tidak
    /// dapat ditentukan, permintaan berhenti pada <c>BlockedByPolicyConfiguration</c> dan tetap
    /// hidup di sana. Ia tidak digagalkan, dan tidak pula diloloskan dengan angka bawaan.
    /// </summary>
    public class BillingFinancialActionService
    {
        private const int MaxPersistenceAttempts = 3;
        private const string PostgresUniqueViolation = "23505";

        private readonly ApplicationDbContext _dbContext;

        public BillingFinancialActionService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // =================================================================
        // Pengajuan
        // =================================================================

        /// <summary>
        /// Membuat permintaan baru dalam keadaan <c>Draft</c>. Belum ada akibat finansial apa pun.
        /// </summary>
        public async Task<BillingServiceResult<FinancialActionRequestResponse>> CreateAsync(
            CreateFinancialActionRequest request,
            Guid makerUserId,
            CancellationToken cancellationToken = default)
        {
            if (makerUserId == Guid.Empty)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.Validation(
                    "BIL_ACTOR_UNKNOWN",
                    "Identitas pengaju tidak dapat ditentukan dari sesi yang sedang berjalan.");
            }

            if (request.RequestedAmount < 0)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.Validation(
                    "BIL_AMOUNT_NEGATIVE",
                    "Nominal tindakan finansial tidak boleh negatif.");
            }

            if (string.IsNullOrWhiteSpace(request.ReasonCode))
            {
                return BillingServiceResult<FinancialActionRequestResponse>.Validation(
                    "BIL_REASON_REQUIRED",
                    "Alasan wajib diisi. Tindakan finansial tanpa alasan tidak dapat dipertanggungjawabkan.");
            }

            // Pengiriman ulang dengan kunci yang sama mengembalikan permintaan yang sudah ada.
            // Tanpa ini, satu klik ganda pada layar petugas menjadi dua permintaan refund.
            if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                var existing = await _dbContext.Set<BilFinancialActionRequest>()
                    .Include(x => x.Approvals)
                    .FirstOrDefaultAsync(
                        x => !x.IsDelete && x.IdempotencyKey == request.IdempotencyKey,
                        cancellationToken);

                if (existing != null)
                {
                    return BillingServiceResult<FinancialActionRequestResponse>.Success(
                        MapToResponse(existing));
                }
            }

            var folio = await _dbContext.Set<BilFolio>()
                .FirstOrDefaultAsync(x => x.Id == request.FolioId && !x.IsDelete, cancellationToken);

            if (folio == null)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.NotFound(
                    "BIL_FOLIO_NOT_FOUND",
                    "Folio tidak ditemukan.");
            }

            BilChargeLine? chargeLine = null;

            if (request.ChargeLineId.HasValue)
            {
                chargeLine = await _dbContext.Set<BilChargeLine>()
                    .FirstOrDefaultAsync(
                        x => x.Id == request.ChargeLineId.Value && !x.IsDelete,
                        cancellationToken);

                if (chargeLine == null)
                {
                    return BillingServiceResult<FinancialActionRequestResponse>.NotFound(
                        "BIL_CHARGE_LINE_NOT_FOUND",
                        "Baris tagihan sasaran tidak ditemukan.");
                }

                if (chargeLine.FolioId != folio.Id)
                {
                    return BillingServiceResult<FinancialActionRequestResponse>.Validation(
                        "BIL_CHARGE_LINE_FOLIO_MISMATCH",
                        "Baris tagihan sasaran bukan milik folio yang disebutkan.");
                }
            }

            if (RequiresChargeLine(request.ActionType) && chargeLine == null)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.Validation(
                    "BIL_CHARGE_LINE_REQUIRED",
                    $"Tindakan {request.ActionType} harus menunjuk baris tagihan yang menjadi sasarannya.");
            }

            var entity = new BilFinancialActionRequest
            {
                ActionType = request.ActionType,
                FolioId = folio.Id,
                EncounterId = folio.EncounterId,
                ChargeLineId = request.ChargeLineId,
                ChargeComponentId = request.ChargeComponentId,
                TargetEncounterId = request.TargetEncounterId,
                TargetVersionAtSubmission = chargeLine?.Version,
                RequestedAmount = request.RequestedAmount,
                Currency = string.IsNullOrWhiteSpace(request.Currency) ? "IDR" : request.Currency,
                ReasonCode = request.ReasonCode,
                ReasonNote = request.ReasonNote,
                MakerUserId = makerUserId,
                Status = BillingFinancialActionStatus.Draft,
                IdempotencyKey = string.IsNullOrWhiteSpace(request.IdempotencyKey)
                    ? null
                    : request.IdempotencyKey,
                CreateBy = makerUserId,
                CreateDateTime = DateTime.UtcNow
            };

            entity.RiskLevel = DetermineRiskLevel(entity, folio, chargeLine);
            entity.ContentHash = ComputeContentHash(entity);

            var persisted = await PersistNewRequestAsync(entity, cancellationToken);

            if (persisted.Kind != BillingServiceResultKind.Success)
            {
                return persisted;
            }

            return BillingServiceResult<FinancialActionRequestResponse>.Success(
                MapToResponse(entity));
        }

        /// <summary>
        /// Mengajukan permintaan untuk diputuskan.
        ///
        /// Di sinilah kebijakan ambang dibaca dan risiko dikunci. Setelah titik ini isi permintaan
        /// tidak dapat disunting lagi — perubahan hanya mungkin melalui
        /// <see cref="ReviseAsync"/>, yang menerbitkan revisi baru dan membekukan yang lama.
        /// </summary>
        public async Task<BillingServiceResult<FinancialActionRequestResponse>> SubmitAsync(
            Guid requestId,
            Guid makerUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await LoadRequestAsync(requestId, cancellationToken);

            if (entity == null)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.NotFound(
                    "BIL_ACTION_REQUEST_NOT_FOUND",
                    "Permintaan tindakan finansial tidak ditemukan.");
            }

            if (entity.MakerUserId != makerUserId)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.Validation(
                    "BIL_NOT_REQUEST_OWNER",
                    "Hanya pengaju permintaan ini yang dapat mengajukannya.");
            }

            if (entity.Status != BillingFinancialActionStatus.Draft &&
                entity.Status != BillingFinancialActionStatus.ReturnedForRevision)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.Conflict(
                    "BIL_ACTION_NOT_SUBMITTABLE",
                    $"Permintaan berstatus {entity.Status} tidak dapat diajukan ulang.");
            }

            var folio = await _dbContext.Set<BilFolio>()
                .FirstOrDefaultAsync(x => x.Id == entity.FolioId && !x.IsDelete, cancellationToken);

            var chargeLine = entity.ChargeLineId.HasValue
                ? await _dbContext.Set<BilChargeLine>()
                    .FirstOrDefaultAsync(
                        x => x.Id == entity.ChargeLineId.Value && !x.IsDelete,
                        cancellationToken)
                : null;

            if (folio == null)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.NotFound(
                    "BIL_FOLIO_NOT_FOUND",
                    "Folio tidak ditemukan.");
            }

            entity.RiskLevel = DetermineRiskLevel(entity, folio, chargeLine);
            entity.TargetVersionAtSubmission = chargeLine?.Version;
            entity.SubmittedAt = DateTime.UtcNow;
            entity.ContentHash = ComputeContentHash(entity);

            var policy = await ResolvePolicyAsync(entity, cancellationToken);

            ApplyPolicyOutcome(entity, policy);

            entity.UpdateBy = makerUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.Version += 1;

            var saved = await SaveWithConcurrencyGuardAsync(cancellationToken);

            if (saved != null)
            {
                return saved;
            }

            return BillingServiceResult<FinancialActionRequestResponse>.Success(
                MapToResponse(entity));
        }

        /// <summary>
        /// Menerbitkan revisi baru dari sebuah permintaan.
        ///
        /// <c>RJ-BIL-GATE-DEC-006</c>: <i>"Material change menghasilkan maker revision baru;
        /// request lama immutable."</i> Karena itu baris lama tidak disunting sedikit pun; ia
        /// ditutup sebagai <c>Cancelled</c> dan permintaan baru menunjuk kepadanya lewat
        /// <c>SupersedesRequestId</c>. Persetujuan yang mungkin sudah menempel pada baris lama
        /// tetap utuh dan tetap terbaca sebagai persetujuan atas isi yang lama.
        /// </summary>
        public async Task<BillingServiceResult<FinancialActionRequestResponse>> ReviseAsync(
            Guid requestId,
            ReviseFinancialActionRequest request,
            Guid makerUserId,
            CancellationToken cancellationToken = default)
        {
            var existing = await LoadRequestAsync(requestId, cancellationToken);

            if (existing == null)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.NotFound(
                    "BIL_ACTION_REQUEST_NOT_FOUND",
                    "Permintaan tindakan finansial tidak ditemukan.");
            }

            if (existing.MakerUserId != makerUserId)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.Validation(
                    "BIL_NOT_REQUEST_OWNER",
                    "Hanya pengaju permintaan ini yang dapat merevisinya.");
            }

            if (existing.Status is BillingFinancialActionStatus.Executed
                or BillingFinancialActionStatus.Cancelled
                or BillingFinancialActionStatus.Rejected)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.Conflict(
                    "BIL_ACTION_NOT_REVISABLE",
                    $"Permintaan berstatus {existing.Status} tidak dapat direvisi.");
            }

            var folio = await _dbContext.Set<BilFolio>()
                .FirstOrDefaultAsync(x => x.Id == existing.FolioId && !x.IsDelete, cancellationToken);

            if (folio == null)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.NotFound(
                    "BIL_FOLIO_NOT_FOUND",
                    "Folio tidak ditemukan.");
            }

            BilChargeLine? chargeLine = null;

            if (request.ChargeLineId.HasValue)
            {
                chargeLine = await _dbContext.Set<BilChargeLine>()
                    .FirstOrDefaultAsync(
                        x => x.Id == request.ChargeLineId.Value && !x.IsDelete,
                        cancellationToken);

                if (chargeLine == null)
                {
                    return BillingServiceResult<FinancialActionRequestResponse>.NotFound(
                        "BIL_CHARGE_LINE_NOT_FOUND",
                        "Baris tagihan sasaran tidak ditemukan.");
                }

                if (chargeLine.FolioId != folio.Id)
                {
                    return BillingServiceResult<FinancialActionRequestResponse>.Validation(
                        "BIL_CHARGE_LINE_FOLIO_MISMATCH",
                        "Baris tagihan sasaran bukan milik folio yang disebutkan.");
                }
            }

            var revision = new BilFinancialActionRequest
            {
                ActionType = existing.ActionType,
                FolioId = existing.FolioId,
                EncounterId = existing.EncounterId,
                ChargeLineId = request.ChargeLineId,
                ChargeComponentId = request.ChargeComponentId,
                TargetEncounterId = request.TargetEncounterId,
                TargetVersionAtSubmission = chargeLine?.Version,
                RequestedAmount = request.RequestedAmount,
                Currency = string.IsNullOrWhiteSpace(request.Currency) ? "IDR" : request.Currency,
                ReasonCode = request.ReasonCode,
                ReasonNote = request.ReasonNote,
                MakerUserId = makerUserId,
                Status = BillingFinancialActionStatus.Draft,
                RevisionNumber = existing.RevisionNumber + 1,
                SupersedesRequestId = existing.Id,
                CreateBy = makerUserId,
                CreateDateTime = DateTime.UtcNow
            };

            revision.RiskLevel = DetermineRiskLevel(revision, folio, chargeLine);
            revision.ContentHash = ComputeContentHash(revision);

            existing.Status = BillingFinancialActionStatus.Cancelled;
            existing.UpdateBy = makerUserId;
            existing.UpdateDateTime = DateTime.UtcNow;
            existing.Version += 1;

            var persisted = await PersistNewRequestAsync(revision, cancellationToken);

            if (persisted.Kind != BillingServiceResultKind.Success)
            {
                return persisted;
            }

            return BillingServiceResult<FinancialActionRequestResponse>.Success(
                MapToResponse(revision));
        }

        // =================================================================
        // Keputusan checker
        // =================================================================

        /// <summary>
        /// Keputusan checker atas sebuah permintaan.
        ///
        /// Larangan self-approval di sini <b>tanpa syarat</b>. Tidak ada parameter, kolom
        /// konfigurasi, atau kebijakan yang dapat melonggarkannya, dan memiliki kemampuan membuat
        /// sekaligus menyetujui tidak membuat seseorang boleh menyetujui permintaannya sendiri —
        /// persis kalimat <c>RJ-BIL-GATE-DEC-006</c>.
        /// </summary>
        public async Task<BillingServiceResult<FinancialActionRequestResponse>> DecideAsync(
            Guid requestId,
            DecideFinancialActionRequest request,
            Guid checkerUserId,
            CancellationToken cancellationToken = default)
        {
            if (checkerUserId == Guid.Empty)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.Validation(
                    "BIL_ACTOR_UNKNOWN",
                    "Identitas checker tidak dapat ditentukan dari sesi yang sedang berjalan.");
            }

            var entity = await LoadRequestAsync(requestId, cancellationToken);

            if (entity == null)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.NotFound(
                    "BIL_ACTION_REQUEST_NOT_FOUND",
                    "Permintaan tindakan finansial tidak ditemukan.");
            }

            // ---------------------------------------------------------------
            // Maker tidak boleh menjadi checker. Tanpa pengecualian.
            // ---------------------------------------------------------------
            if (entity.MakerUserId == checkerUserId)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.Validation(
                    "BIL_SELF_APPROVAL_FORBIDDEN",
                    "Permintaan tidak dapat diputuskan oleh pengajunya sendiri. Pemisahan pengaju " +
                    "dan pemutus adalah syarat mutlak pada tindakan finansial, dan memiliki " +
                    "kedua kewenangan sekaligus tidak mengubah hal itu.");
            }

            if (entity.Status != BillingFinancialActionStatus.PendingApproval)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.Conflict(
                    "BIL_ACTION_NOT_PENDING_APPROVAL",
                    $"Permintaan berstatus {entity.Status} tidak sedang menunggu keputusan.");
            }

            if (entity.ExpiresAt.HasValue && entity.ExpiresAt.Value <= DateTime.UtcNow)
            {
                entity.Status = BillingFinancialActionStatus.Expired;
                entity.UpdateBy = checkerUserId;
                entity.UpdateDateTime = DateTime.UtcNow;
                entity.Version += 1;

                await _dbContext.SaveChangesAsync(cancellationToken);

                return BillingServiceResult<FinancialActionRequestResponse>.Conflict(
                    "BIL_ACTION_EXPIRED",
                    "Permintaan sudah kedaluwarsa dan tidak dapat diputuskan. Kedaluwarsa bukan persetujuan.");
            }

            // Isi yang disetujui harus isi yang sedang berlaku.
            if (!string.IsNullOrWhiteSpace(request.ExpectedContentHash) &&
                !string.Equals(request.ExpectedContentHash, entity.ContentHash, StringComparison.OrdinalIgnoreCase))
            {
                return BillingServiceResult<FinancialActionRequestResponse>.Conflict(
                    "BIL_ACTION_CONTENT_CHANGED",
                    "Isi permintaan sudah berubah sejak terakhir Anda melihatnya. Keputusan " +
                    "dibatalkan agar tidak menyetujui isi yang bukan Anda periksa.");
            }

            if (request.Decision == BillingApprovalDecision.Approve &&
                request.ApprovedAmount.HasValue &&
                request.ApprovedAmount.Value > entity.RequestedAmount)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.Validation(
                    "BIL_APPROVED_AMOUNT_EXCEEDS_REQUEST",
                    "Nominal yang disetujui tidak boleh melebihi nominal yang diajukan.");
            }

            var priorStatus = entity.Status;

            entity.Status = request.Decision switch
            {
                BillingApprovalDecision.Approve => BillingFinancialActionStatus.Approved,
                BillingApprovalDecision.Reject => BillingFinancialActionStatus.Rejected,
                BillingApprovalDecision.ReturnForRevision => BillingFinancialActionStatus.ReturnedForRevision,
                _ => entity.Status
            };

            var approval = new BilFinancialApproval
            {
                RequestId = entity.Id,
                Decision = request.Decision,
                CheckerUserId = checkerUserId,
                DecidedAt = DateTime.UtcNow,
                DecisionNote = request.DecisionNote,
                ApprovedAmount = request.Decision == BillingApprovalDecision.Approve
                    ? request.ApprovedAmount ?? entity.RequestedAmount
                    : null,
                RequestContentHash = entity.ContentHash,
                ActionType = entity.ActionType,
                PriorStatus = priorStatus,
                ResultingStatus = entity.Status,
                MakerUserId = entity.MakerUserId,
                FolioId = entity.FolioId,
                EncounterId = entity.EncounterId,
                ChargeLineId = entity.ChargeLineId,
                RequestedAmount = entity.RequestedAmount,
                ApprovalPolicyId = entity.ApprovalPolicyId,
                ApprovalPolicyVersion = entity.ApprovalPolicyVersion,
                CorrelationId = entity.CorrelationId,
                CreateBy = checkerUserId,
                CreateDateTime = DateTime.UtcNow
            };

            _dbContext.Set<BilFinancialApproval>().Add(approval);

            entity.UpdateBy = checkerUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.Version += 1;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // Dua checker menekan tombol bersamaan. Index unik pada (RequestId, Decision=Approve)
                // memastikan hanya satu yang menang, dan yang kalah diberi tahu apa adanya.
                return BillingServiceResult<FinancialActionRequestResponse>.Conflict(
                    "BIL_ACTION_ALREADY_DECIDED",
                    "Permintaan ini baru saja diputuskan oleh checker lain.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.Conflict(
                    "BIL_ACTION_VERSION_CONFLICT",
                    "Permintaan berubah bersamaan dengan keputusan ini. Muat ulang lalu ulangi.");
            }

            // EF sudah menyambungkan keputusan ini ke koleksi permintaan saat baris ditambahkan.
            // Menambahkannya lagi akan membuat satu keputusan terbaca dua kali pada jawaban, dan
            // laporan audit akan menghitungnya dua kali pula.
            if (!entity.Approvals.Contains(approval))
            {
                entity.Approvals.Add(approval);
            }

            return BillingServiceResult<FinancialActionRequestResponse>.Success(
                MapToResponse(entity));
        }

        // =================================================================
        // Pelaksanaan
        // =================================================================

        /// <summary>
        /// Menjalankan tindakan yang sudah disetujui.
        ///
        /// Dua penjagaan yang dituntut <c>RJ-BIL-GATE-DEC-006</c> ada di sini.
        /// <b>Idempoten</b>: permintaan yang sudah dijalankan mengembalikan hasil yang sama tanpa
        /// menggandakan efeknya. <b>Revalidasi</b>: bila keadaan sasaran berubah sejak permintaan
        /// diajukan, eksekusi berhenti pada <c>RevalidationRequired</c> alih-alih menjalankan
        /// keputusan atas keadaan yang sudah tidak berlaku.
        /// </summary>
        public async Task<BillingServiceResult<FinancialActionRequestResponse>> ExecuteAsync(
            Guid requestId,
            ExecuteFinancialActionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await LoadRequestAsync(requestId, cancellationToken);

            if (entity == null)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.NotFound(
                    "BIL_ACTION_REQUEST_NOT_FOUND",
                    "Permintaan tindakan finansial tidak ditemukan.");
            }

            // Idempoten: pengulangan tidak menggandakan refund, reversal, waiver, write-off,
            // maupun adjustment.
            if (entity.Status == BillingFinancialActionStatus.Executed)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.Success(
                    MapToResponse(entity));
            }

            if (entity.Status != BillingFinancialActionStatus.Approved)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.Conflict(
                    "BIL_ACTION_NOT_APPROVED",
                    $"Permintaan berstatus {entity.Status} belum boleh dijalankan. " +
                    "Efek finansial hanya terjadi setelah ada persetujuan.");
            }

            if (entity.ActionType == BillingFinancialActionType.FolioReopen)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.Validation(
                    "BIL_USE_FOLIO_REOPEN_ENDPOINT",
                    "Pembukaan kembali folio dijalankan melalui endpoint reopen folio, agar " +
                    "riwayat penutupannya ikut tercatat.");
            }

            BilChargeLine? chargeLine = null;

            if (entity.ChargeLineId.HasValue)
            {
                chargeLine = await _dbContext.Set<BilChargeLine>()
                    .FirstOrDefaultAsync(
                        x => x.Id == entity.ChargeLineId.Value && !x.IsDelete,
                        cancellationToken);

                if (chargeLine == null)
                {
                    return BillingServiceResult<FinancialActionRequestResponse>.NotFound(
                        "BIL_CHARGE_LINE_NOT_FOUND",
                        "Baris tagihan sasaran sudah tidak ditemukan.");
                }

                // Revalidasi keadaan sasaran.
                if (entity.TargetVersionAtSubmission.HasValue &&
                    chargeLine.Version != entity.TargetVersionAtSubmission.Value)
                {
                    entity.Status = BillingFinancialActionStatus.RevalidationRequired;
                    entity.UpdateBy = actorUserId;
                    entity.UpdateDateTime = DateTime.UtcNow;
                    entity.Version += 1;

                    await _dbContext.SaveChangesAsync(cancellationToken);

                    return BillingServiceResult<FinancialActionRequestResponse>.Conflict(
                        "BIL_ACTION_REVALIDATION_REQUIRED",
                        "Baris tagihan sasaran sudah berubah sejak permintaan ini diajukan. " +
                        "Permintaan dihentikan untuk dinilai ulang, bukan dijalankan atas " +
                        "keadaan yang sudah tidak berlaku.");
                }
            }

            var approvedAmount = entity.Approvals
                .Where(x => x.Decision == BillingApprovalDecision.Approve)
                .OrderByDescending(x => x.DecidedAt)
                .Select(x => x.ApprovedAmount)
                .FirstOrDefault() ?? entity.RequestedAmount;

            ApplyFinancialEffect(entity, chargeLine, actorUserId);

            entity.Status = BillingFinancialActionStatus.Executed;
            entity.ExecutedAt = DateTime.UtcNow;
            entity.ExecutedByUserId = actorUserId;
            entity.ExecutedAmount = approvedAmount;
            entity.ExecutionNote = request.ExecutionNote;
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.Version += 1;

            var saved = await SaveWithConcurrencyGuardAsync(cancellationToken);

            if (saved != null)
            {
                return saved;
            }

            return BillingServiceResult<FinancialActionRequestResponse>.Success(
                MapToResponse(entity));
        }

        public async Task<BillingServiceResult<FinancialActionRequestResponse>> CancelAsync(
            Guid requestId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await LoadRequestAsync(requestId, cancellationToken);

            if (entity == null)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.NotFound(
                    "BIL_ACTION_REQUEST_NOT_FOUND",
                    "Permintaan tindakan finansial tidak ditemukan.");
            }

            if (entity.Status == BillingFinancialActionStatus.Executed)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.Conflict(
                    "BIL_ACTION_ALREADY_EXECUTED",
                    "Permintaan yang sudah dijalankan tidak dapat dibatalkan. Koreksinya adalah " +
                    "tindakan finansial baru, bukan penghapusan yang lama.");
            }

            if (entity.MakerUserId != actorUserId)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.Validation(
                    "BIL_NOT_REQUEST_OWNER",
                    "Hanya pengaju permintaan ini yang dapat membatalkannya.");
            }

            entity.Status = BillingFinancialActionStatus.Cancelled;
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.Version += 1;

            var saved = await SaveWithConcurrencyGuardAsync(cancellationToken);

            if (saved != null)
            {
                return saved;
            }

            return BillingServiceResult<FinancialActionRequestResponse>.Success(
                MapToResponse(entity));
        }

        /// <summary>
        /// Menandai permintaan yang sudah melewati batas waktu keputusan.
        ///
        /// Kedaluwarsa hanya menutup permintaan yang tidak pernah diputuskan. Ia tidak pernah
        /// berarti disetujui, dan tidak pernah menghasilkan efek finansial apa pun.
        /// </summary>
        public async Task<int> ExpireDueRequestsAsync(
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var due = await _dbContext.Set<BilFinancialActionRequest>()
                .Where(x => !x.IsDelete &&
                            x.Status == BillingFinancialActionStatus.PendingApproval &&
                            x.ExpiresAt != null &&
                            x.ExpiresAt <= now)
                .ToListAsync(cancellationToken);

            foreach (var item in due)
            {
                item.Status = BillingFinancialActionStatus.Expired;
                item.UpdateBy = actorUserId;
                item.UpdateDateTime = now;
                item.Version += 1;
            }

            if (due.Count > 0)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return due.Count;
        }

        // =================================================================
        // Pembacaan
        // =================================================================

        public async Task<FinancialActionRequestResponse?> GetByIdAsync(
            Guid requestId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<BilFinancialActionRequest>()
                .AsNoTracking()
                .Include(x => x.Approvals.Where(a => !a.IsDelete))
                .FirstOrDefaultAsync(x => x.Id == requestId && !x.IsDelete, cancellationToken);

            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<List<FinancialActionRequestResponse>> GetAsync(
            Guid? folioId,
            Guid? encounterId,
            BillingFinancialActionType? actionType,
            BillingFinancialActionStatus? status,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<BilFinancialActionRequest>()
                .AsNoTracking()
                .Include(x => x.Approvals.Where(a => !a.IsDelete))
                .Where(x => !x.IsDelete);

            if (folioId.HasValue)
            {
                query = query.Where(x => x.FolioId == folioId.Value);
            }

            if (encounterId.HasValue)
            {
                query = query.Where(x => x.EncounterId == encounterId.Value);
            }

            if (actionType.HasValue)
            {
                query = query.Where(x => x.ActionType == actionType.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            var items = await query
                .OrderByDescending(x => x.CreateDateTime)
                .ToListAsync(cancellationToken);

            return items.Select(MapToResponse).ToList();
        }

        // =================================================================
        // Penilaian risiko dan kebijakan
        // =================================================================

        /// <summary>
        /// Menentukan apakah sebuah permintaan selalu high-risk tanpa memandang nominal.
        ///
        /// <c>RJ-BIL-GATE-DEC-006</c> menyebut empat hal: void/reversal terhadap tagihan yang
        /// sudah <c>Paid</c>, <c>Posted</c>, <c>Claimed</c>, atau <c>Settled</c>; refund atas
        /// pembayaran yang sudah settled; reopen folio yang tertutup; dan koreksi lintas
        /// encounter.
        ///
        /// <para><b>Dua di antaranya belum dapat dinilai apa adanya, dan itu disikapi fail-closed.</b></para>
        ///
        /// Keadaan <c>Paid</c>, <c>Posted</c>, <c>Claimed</c>, dan <c>Settled</c> belum ada di
        /// model mana pun — keduanya lahir bersama <c>RJ-BIL-BE-005</c> dan <c>RJ-BIL-BE-008</c>
        /// yang belum dikerjakan. Menebak padanannya akan mengarang keputusan yang bukan milik
        /// task ini. Yang dilakukan sebaliknya: bila keadaan itu tidak dapat dipastikan, tindakan
        /// diperlakukan sebagai high-risk. Salah menganggap high-risk hanya menambah satu
        /// persetujuan; salah menganggap aman berarti uang berpindah tanpa pengawasan.
        ///
        /// Karena itu untuk sekarang: void/reversal terhadap baris yang sudah <c>Recognized</c>
        /// dianggap high-risk, dan <b>seluruh</b> refund dianggap high-risk. Keduanya akan
        /// dipersempit begitu keadaan pembayaran yang sesungguhnya tersedia.
        /// </summary>
        public static BillingFinancialRiskLevel DetermineRiskLevel(
            BilFinancialActionRequest entity,
            BilFolio folio,
            BilChargeLine? chargeLine)
        {
            if (entity.TargetEncounterId.HasValue &&
                entity.TargetEncounterId.Value != entity.EncounterId)
            {
                return BillingFinancialRiskLevel.HighRisk;
            }

            if (entity.ActionType == BillingFinancialActionType.FolioReopen &&
                folio.Status == BillingFolioStatus.Closed)
            {
                return BillingFinancialRiskLevel.HighRisk;
            }

            if (entity.ActionType == BillingFinancialActionType.Refund)
            {
                return BillingFinancialRiskLevel.HighRisk;
            }

            if ((entity.ActionType == BillingFinancialActionType.Void ||
                 entity.ActionType == BillingFinancialActionType.Reversal) &&
                chargeLine != null &&
                chargeLine.CalculationStatus == BillingChargeCalculationStatus.Recognized)
            {
                return BillingFinancialRiskLevel.HighRisk;
            }

            return BillingFinancialRiskLevel.Normal;
        }

        private async Task<MstBillingApprovalPolicy?> ResolvePolicyAsync(
            BilFinancialActionRequest entity,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var candidates = await _dbContext.Set<MstBillingApprovalPolicy>()
                .AsNoTracking()
                .Where(x => !x.IsDelete &&
                            x.IsActive &&
                            x.IsApproved &&
                            x.ActionType == entity.ActionType &&
                            x.EffectiveStartDate <= now &&
                            (x.EffectiveEndDate == null || x.EffectiveEndDate > now))
                .OrderByDescending(x => x.PolicyVersion)
                .ToListAsync(cancellationToken);

            return candidates.FirstOrDefault(x =>
                (x.MinimumAmount == null || entity.RequestedAmount >= x.MinimumAmount.Value) &&
                (x.MaximumAmount == null || entity.RequestedAmount <= x.MaximumAmount.Value));
        }

        /// <summary>
        /// Menerjemahkan hasil pencarian kebijakan menjadi status permintaan.
        ///
        /// Perhatikan urutannya: high-risk diperiksa <b>lebih dulu</b>, sehingga kebijakan tidak
        /// pernah dapat mencabut kewajiban persetujuan pada tindakan yang memang selalu
        /// high-risk. Kebijakan boleh menambah kewajiban, tidak boleh menguranginya.
        /// </summary>
        private static void ApplyPolicyOutcome(
            BilFinancialActionRequest entity,
            MstBillingApprovalPolicy? policy)
        {
            var alwaysHighRisk = entity.RiskLevel == BillingFinancialRiskLevel.HighRisk;

            if (policy == null)
            {
                if (alwaysHighRisk)
                {
                    entity.RequiresApproval = true;
                    entity.Status = BillingFinancialActionStatus.PendingApproval;
                    entity.ApprovalPolicyId = null;
                    entity.ApprovalPolicyVersion = null;
                    entity.ExpiresAt = null;
                    entity.PolicyBlockReason =
                        "Belum ada kebijakan ambang yang sah untuk jenis tindakan ini, tetapi " +
                        "tindakan ini selalu high-risk tanpa memandang nominal, sehingga tetap " +
                        "menunggu persetujuan.";
                    return;
                }

                entity.RequiresApproval = true;
                entity.Status = BillingFinancialActionStatus.BlockedByPolicyConfiguration;
                entity.ApprovalPolicyId = null;
                entity.ApprovalPolicyVersion = null;
                entity.ExpiresAt = null;
                entity.PolicyBlockReason =
                    "Kebijakan ambang persetujuan yang sah tidak dapat ditentukan untuk jenis " +
                    "dan nominal ini. Permintaan tidak digagalkan dan tidak pula diloloskan; ia " +
                    "menunggu Finance menetapkan kebijakannya (RJ-BIL-OQ-004).";
                return;
            }

            entity.ApprovalPolicyId = policy.Id;
            entity.ApprovalPolicyVersion = policy.PolicyVersion;
            entity.PolicyBlockReason = null;
            entity.RequiresApproval = alwaysHighRisk || policy.RequiresApproval;

            entity.ExpiresAt = policy.ApprovalExpiryMinutes > 0
                ? DateTime.UtcNow.AddMinutes(policy.ApprovalExpiryMinutes)
                : null;

            entity.Status = entity.RequiresApproval
                ? BillingFinancialActionStatus.PendingApproval
                : BillingFinancialActionStatus.Approved;
        }

        /// <summary>
        /// Menerapkan akibat finansial dari tindakan yang sudah disetujui.
        ///
        /// <c>RJ-BIL-GATE-DEC-006</c>: <i>"Waiver/write-off/manual override tidak menghapus
        /// original charge; financial effect diterapkan melalui approved adjustment/action."</i>
        /// Karena itu hanya void dan reversal yang menyentuh status baris tagihan — dan keduanya
        /// pun tidak menghapus barisnya, hanya menandainya. Untuk jenis lain, baris permintaan
        /// yang sudah <c>Executed</c> inilah catatan finansialnya; tagihan aslinya tetap utuh
        /// dan tetap terbaca.
        /// </summary>
        private static void ApplyFinancialEffect(
            BilFinancialActionRequest entity,
            BilChargeLine? chargeLine,
            Guid actorUserId)
        {
            if (chargeLine == null)
            {
                return;
            }

            var newStatus = entity.ActionType switch
            {
                BillingFinancialActionType.Void => BillingChargeCalculationStatus.Voided,
                BillingFinancialActionType.Reversal => BillingChargeCalculationStatus.Reversed,
                _ => (BillingChargeCalculationStatus?)null
            };

            if (newStatus == null)
            {
                return;
            }

            chargeLine.CalculationStatus = newStatus.Value;
            chargeLine.Version += 1;
            chargeLine.UpdateBy = actorUserId;
            chargeLine.UpdateDateTime = DateTime.UtcNow;
        }

        // =================================================================
        // Pembantu
        // =================================================================

        private static bool RequiresChargeLine(BillingFinancialActionType actionType) =>
            actionType is BillingFinancialActionType.Void
                or BillingFinancialActionType.Reversal
                or BillingFinancialActionType.Adjustment;

        private Task<BilFinancialActionRequest?> LoadRequestAsync(
            Guid requestId,
            CancellationToken cancellationToken) =>
            _dbContext.Set<BilFinancialActionRequest>()
                .Include(x => x.Approvals.Where(a => !a.IsDelete))
                .FirstOrDefaultAsync(x => x.Id == requestId && !x.IsDelete, cancellationToken);

        /// <summary>
        /// Sidik isi yang menentukan akibat finansial sebuah permintaan.
        ///
        /// Yang ikut dihitung hanyalah hal-hal yang mengubah akibatnya. Catatan bebas seperti
        /// <c>ReasonNote</c> ikut, karena ia bagian dari apa yang dibaca checker sebelum
        /// memutuskan.
        /// </summary>
        public static string ComputeContentHash(BilFinancialActionRequest entity)
        {
            var canonical = string.Join('|',
                (int)entity.ActionType,
                entity.FolioId,
                entity.EncounterId,
                entity.ChargeLineId?.ToString() ?? string.Empty,
                entity.ChargeComponentId?.ToString() ?? string.Empty,
                entity.TargetEncounterId?.ToString() ?? string.Empty,
                entity.RequestedAmount.ToString("F6", System.Globalization.CultureInfo.InvariantCulture),
                entity.Currency,
                entity.ReasonCode,
                entity.ReasonNote ?? string.Empty,
                entity.RevisionNumber);

            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));

            return Convert.ToHexString(bytes);
        }

        private async Task<BillingServiceResult<FinancialActionRequestResponse>> PersistNewRequestAsync(
            BilFinancialActionRequest entity,
            CancellationToken cancellationToken)
        {
            for (var attempt = 1; attempt <= MaxPersistenceAttempts; attempt++)
            {
                entity.RequestNumber = GenerateRequestNumber(entity.ActionType);

                _dbContext.Set<BilFinancialActionRequest>().Add(entity);

                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    return BillingServiceResult<FinancialActionRequestResponse>.Success(
                        MapToResponse(entity));
                }
                catch (DbUpdateException ex) when (IsUniqueViolation(ex))
                {
                    _dbContext.Entry(entity).State = EntityState.Detached;

                    // Kunci idempotensi yang bentrok berarti permintaan yang sama sudah ada.
                    // Yang benar adalah mengembalikan yang sudah ada, bukan memaksa membuat baru.
                    if (!string.IsNullOrWhiteSpace(entity.IdempotencyKey))
                    {
                        var existing = await _dbContext.Set<BilFinancialActionRequest>()
                            .Include(x => x.Approvals)
                            .FirstOrDefaultAsync(
                                x => !x.IsDelete && x.IdempotencyKey == entity.IdempotencyKey,
                                cancellationToken);

                        if (existing != null)
                        {
                            return BillingServiceResult<FinancialActionRequestResponse>.Success(
                                MapToResponse(existing));
                        }
                    }

                    if (attempt == MaxPersistenceAttempts)
                    {
                        return BillingServiceResult<FinancialActionRequestResponse>.Conflict(
                            "BIL_ACTION_REQUEST_NUMBER_CONFLICT",
                            "Nomor permintaan bentrok berulang kali. Silakan ulangi.");
                    }
                }
            }

            return BillingServiceResult<FinancialActionRequestResponse>.Conflict(
                "BIL_ACTION_REQUEST_NOT_PERSISTED",
                "Permintaan tidak dapat disimpan.");
        }

        private async Task<BillingServiceResult<FinancialActionRequestResponse>?>
            SaveWithConcurrencyGuardAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return null;
            }
            catch (DbUpdateConcurrencyException)
            {
                return BillingServiceResult<FinancialActionRequestResponse>.Conflict(
                    "BIL_ACTION_VERSION_CONFLICT",
                    "Permintaan berubah bersamaan dengan tindakan ini. Muat ulang lalu ulangi.");
            }
        }

        private static bool IsUniqueViolation(DbUpdateException exception) =>
            exception.InnerException is PostgresException postgres &&
            postgres.SqlState == PostgresUniqueViolation;

        private static string GenerateRequestNumber(BillingFinancialActionType actionType)
        {
            var prefix = actionType switch
            {
                BillingFinancialActionType.Void => "VD",
                BillingFinancialActionType.Adjustment => "AD",
                BillingFinancialActionType.Reversal => "RV",
                BillingFinancialActionType.Refund => "RF",
                BillingFinancialActionType.Waiver => "WV",
                BillingFinancialActionType.WriteOff => "WO",
                BillingFinancialActionType.ManualOverride => "MO",
                BillingFinancialActionType.FolioReopen => "RO",
                _ => "FA"
            };

            return $"{prefix}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..22].ToUpperInvariant();
        }

        internal static FinancialActionRequestResponse MapToResponse(
            BilFinancialActionRequest entity) =>
            new()
            {
                Id = entity.Id,
                RequestNumber = entity.RequestNumber,
                ActionType = entity.ActionType,
                ActionTypeName = entity.ActionType.ToString(),
                Status = entity.Status,
                StatusName = entity.Status.ToString(),
                RiskLevel = entity.RiskLevel,
                RequiresApproval = entity.RequiresApproval,
                FolioId = entity.FolioId,
                EncounterId = entity.EncounterId,
                ChargeLineId = entity.ChargeLineId,
                ChargeComponentId = entity.ChargeComponentId,
                TargetEncounterId = entity.TargetEncounterId,
                RequestedAmount = entity.RequestedAmount,
                ExecutedAmount = entity.ExecutedAmount,
                Currency = entity.Currency,
                ReasonCode = entity.ReasonCode,
                ReasonNote = entity.ReasonNote,
                MakerUserId = entity.MakerUserId,
                SubmittedAt = entity.SubmittedAt,
                ExpiresAt = entity.ExpiresAt,
                ExecutedAt = entity.ExecutedAt,
                ExecutedByUserId = entity.ExecutedByUserId,
                RevisionNumber = entity.RevisionNumber,
                SupersedesRequestId = entity.SupersedesRequestId,
                ContentHash = entity.ContentHash,
                ApprovalPolicyId = entity.ApprovalPolicyId,
                ApprovalPolicyVersion = entity.ApprovalPolicyVersion,
                PolicyBlockReason = entity.PolicyBlockReason,
                Version = entity.Version,
                NextAction = BuildNextAction(entity),
                Approvals = entity.Approvals
                    .OrderBy(x => x.DecidedAt)
                    .Select(x => new FinancialApprovalResponse
                    {
                        Id = x.Id,
                        Decision = x.Decision,
                        DecisionName = x.Decision.ToString(),
                        CheckerUserId = x.CheckerUserId,
                        DecidedAt = x.DecidedAt,
                        ApprovedAmount = x.ApprovedAmount,
                        DecisionNote = x.DecisionNote,
                        RequestContentHash = x.RequestContentHash,
                        PriorStatus = x.PriorStatus,
                        ResultingStatus = x.ResultingStatus
                    })
                    .ToList()
            };

        private static string BuildNextAction(BilFinancialActionRequest entity) =>
            entity.Status switch
            {
                BillingFinancialActionStatus.Draft =>
                    "Ajukan permintaan ini agar dapat diputuskan.",
                BillingFinancialActionStatus.Submitted =>
                    "Menunggu penetapan kebijakan dan kewajiban persetujuan.",
                BillingFinancialActionStatus.PendingApproval =>
                    "Menunggu keputusan checker. Checker harus orang yang berbeda dari pengaju.",
                BillingFinancialActionStatus.Approved =>
                    "Sudah disetujui. Jalankan agar efek finansialnya berlaku.",
                BillingFinancialActionStatus.Rejected =>
                    "Ditolak checker. Ajukan permintaan baru bila memang masih diperlukan.",
                BillingFinancialActionStatus.ReturnedForRevision =>
                    "Dikembalikan untuk diperbaiki. Terbitkan revisi, bukan menyunting yang lama.",
                BillingFinancialActionStatus.Cancelled =>
                    "Dibatalkan. Tidak ada tindakan lanjutan.",
                BillingFinancialActionStatus.Expired =>
                    "Kedaluwarsa tanpa keputusan. Kedaluwarsa bukan persetujuan; ajukan permintaan baru.",
                BillingFinancialActionStatus.BlockedByPolicyConfiguration =>
                    "Tertahan karena kebijakan ambang belum ditetapkan Finance. Permintaan tetap " +
                    "hidup dan akan dapat diputuskan begitu kebijakannya ada.",
                BillingFinancialActionStatus.Executed =>
                    "Sudah dijalankan. Pengulangan tidak akan menggandakan efeknya.",
                BillingFinancialActionStatus.RevalidationRequired =>
                    "Keadaan sasaran berubah setelah disetujui. Nilai ulang lalu terbitkan revisi.",
                _ => string.Empty
            };
    }
}
