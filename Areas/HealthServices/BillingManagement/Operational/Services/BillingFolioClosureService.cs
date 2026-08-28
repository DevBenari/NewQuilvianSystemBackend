using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services
{
    /// <summary>
    /// Penutupan dan pembukaan kembali folio — <c>RJ-BIL-BE-006</c>, melaksanakan
    /// <c>RJ-BIL-GATE-DEC-006</c>.
    ///
    /// <para><b>Penutupan folio adalah pernyataan bahwa tidak ada lagi uang yang belum jelas.</b></para>
    ///
    /// Karena itu layanan ini tidak menutup folio atas penilaiannya sendiri. Ia bertanya lebih
    /// dulu kepada gerbang kesiapan milik <c>RJ-BIL-BE-007</c>, lalu menambahkan satu pertanyaan
    /// yang menjadi ranahnya sendiri: <i>apakah masih ada permintaan tindakan finansial yang
    /// belum selesai?</i> Permintaan yang menggantung berarti angka folio masih bisa berubah,
    /// dan menutupnya sekarang akan membuat perubahan itu terjadi di belakang folio yang sudah
    /// dinyatakan selesai.
    ///
    /// <para><b>Membuka kembali tidak menghapus riwayat penutupan.</b></para>
    ///
    /// Setiap penutupan dan pembukaan menjadi barisnya sendiri pada
    /// <c>BilFolioClosureHistory</c>, lengkap dengan bukti keadaan gerbang saat itu.
    /// </summary>
    public class BillingFolioClosureService
    {
        /// <summary>
        /// Status permintaan yang dianggap <b>belum selesai</b>, sehingga menahan penutupan folio.
        ///
        /// <c>Approved</c> sengaja termasuk. Permintaan yang sudah disetujui tetapi belum
        /// dijalankan berarti angka folio masih akan berubah; menutupnya sekarang sama saja
        /// menyatakan selesai atas sesuatu yang jelas-jelas belum.
        /// </summary>
        private static readonly BillingFinancialActionStatus[] UnsettledStatuses =
        {
            BillingFinancialActionStatus.Draft,
            BillingFinancialActionStatus.Submitted,
            BillingFinancialActionStatus.PendingApproval,
            BillingFinancialActionStatus.Approved,
            BillingFinancialActionStatus.BlockedByPolicyConfiguration,
            BillingFinancialActionStatus.RevalidationRequired
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly BillingReconciliationService _reconciliationService;

        public BillingFolioClosureService(
            ApplicationDbContext dbContext,
            BillingReconciliationService reconciliationService)
        {
            _dbContext = dbContext;
            _reconciliationService = reconciliationService;
        }

        /// <summary>
        /// Menutup folio, bila dan hanya bila tidak ada satu pun hal yang menahannya.
        /// </summary>
        public async Task<BillingServiceResult<FolioClosureResponse>> CloseAsync(
            Guid folioId,
            CloseFolioRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
            {
                return BillingServiceResult<FolioClosureResponse>.Validation(
                    "BIL_ACTOR_UNKNOWN",
                    "Identitas petugas tidak dapat ditentukan dari sesi yang sedang berjalan.");
            }

            var folio = await _dbContext.Set<BilFolio>()
                .FirstOrDefaultAsync(x => x.Id == folioId && !x.IsDelete, cancellationToken);

            if (folio == null)
            {
                return BillingServiceResult<FolioClosureResponse>.NotFound(
                    "BIL_FOLIO_NOT_FOUND",
                    "Folio tidak ditemukan.");
            }

            if (folio.Status == BillingFolioStatus.Closed)
            {
                return BillingServiceResult<FolioClosureResponse>.Conflict(
                    "BIL_FOLIO_ALREADY_CLOSED",
                    "Folio sudah tertutup.");
            }

            // Gerbang milik RJ-BIL-BE-007: rekonsiliasi, hasil pemrosesan yang belum pasti, dan
            // baris tagihan yang masih menunggu telaah finansial.
            var readiness = await _reconciliationService.EvaluateClosureReadinessAsync(
                folioId, cancellationToken);

            if (readiness.Kind != BillingServiceResultKind.Success || readiness.Value == null)
            {
                return BillingServiceResult<FolioClosureResponse>.NotFound(
                    readiness.ErrorCode ?? "BIL_FOLIO_NOT_FOUND",
                    readiness.ErrorMessage ?? "Kesiapan penutupan folio tidak dapat dinilai.");
            }

            var blockerDescriptions = readiness.Value.Blockers
                .Select(x => $"{x.BlockerCode}: {x.Description}")
                .ToList();

            // Gerbang milik task ini: permintaan tindakan finansial yang belum selesai.
            var unsettled = await _dbContext.Set<BilFinancialActionRequest>()
                .AsNoTracking()
                .Where(x => !x.IsDelete &&
                            x.FolioId == folioId &&
                            UnsettledStatuses.Contains(x.Status))
                .Select(x => new { x.RequestNumber, x.ActionType, x.Status })
                .ToListAsync(cancellationToken);

            foreach (var item in unsettled)
            {
                blockerDescriptions.Add(
                    $"FINANCIAL_ACTION_{item.Status}".ToUpperInvariant() +
                    $": Permintaan {item.RequestNumber} ({item.ActionType}) belum selesai.");
            }

            if (blockerDescriptions.Count > 0)
            {
                return BillingServiceResult<FolioClosureResponse>.Conflict(
                    "BIL_FOLIO_CLOSE_BLOCKED",
                    "Folio tidak dapat ditutup karena masih ada yang menahannya: " +
                    string.Join(" | ", blockerDescriptions));
            }

            var priorStatus = folio.Status;

            folio.Status = BillingFolioStatus.Closed;
            folio.Version += 1;
            folio.UpdateBy = actorUserId;
            folio.UpdateDateTime = DateTime.UtcNow;

            var history = new BilFolioClosureHistory
            {
                FolioId = folio.Id,
                EncounterId = folio.EncounterId,
                Action = BillingFolioClosureAction.Close,
                PriorStatus = priorStatus,
                NewStatus = BillingFolioStatus.Closed,
                PerformedByUserId = actorUserId,
                PerformedAt = DateTime.UtcNow,
                Note = request.Note,
                ClosureEvidence = BuildClosureEvidence(readiness.Value, unsettled.Count),
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };

            _dbContext.Set<BilFolioClosureHistory>().Add(history);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return BillingServiceResult<FolioClosureResponse>.Conflict(
                    "BIL_FOLIO_VERSION_CONFLICT",
                    "Folio berubah bersamaan dengan penutupan ini. Muat ulang lalu ulangi.");
            }

            return BillingServiceResult<FolioClosureResponse>.Success(new FolioClosureResponse
            {
                FolioId = folio.Id,
                EncounterId = folio.EncounterId,
                Status = folio.Status,
                StatusName = folio.Status.ToString(),
                Version = folio.Version,
                PerformedAt = history.PerformedAt,
                PerformedByUserId = actorUserId,
                Message = "Folio ditutup. Tidak ada penghalang yang tersisa pada saat penutupan."
            });
        }

        /// <summary>
        /// Membuka kembali folio yang sudah tertutup.
        ///
        /// <c>RJ-BIL-GATE-DEC-006</c>: <i>"Reopen selalu controlled high-risk request."</i> Karena
        /// itu endpoint ini tidak menerima alasan bebas sebagai dasar — yang diterimanya adalah
        /// sebuah permintaan <c>FolioReopen</c> yang sudah melewati maker-checker.
        /// </summary>
        public async Task<BillingServiceResult<FolioClosureResponse>> ReopenAsync(
            Guid folioId,
            ReopenFolioRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
            {
                return BillingServiceResult<FolioClosureResponse>.Validation(
                    "BIL_ACTOR_UNKNOWN",
                    "Identitas petugas tidak dapat ditentukan dari sesi yang sedang berjalan.");
            }

            var folio = await _dbContext.Set<BilFolio>()
                .FirstOrDefaultAsync(x => x.Id == folioId && !x.IsDelete, cancellationToken);

            if (folio == null)
            {
                return BillingServiceResult<FolioClosureResponse>.NotFound(
                    "BIL_FOLIO_NOT_FOUND",
                    "Folio tidak ditemukan.");
            }

            if (folio.Status != BillingFolioStatus.Closed)
            {
                return BillingServiceResult<FolioClosureResponse>.Conflict(
                    "BIL_FOLIO_NOT_CLOSED",
                    "Folio tidak sedang tertutup, sehingga tidak ada yang perlu dibuka kembali.");
            }

            var actionRequest = await _dbContext.Set<BilFinancialActionRequest>()
                .FirstOrDefaultAsync(
                    x => x.Id == request.FinancialActionRequestId && !x.IsDelete,
                    cancellationToken);

            if (actionRequest == null)
            {
                return BillingServiceResult<FolioClosureResponse>.NotFound(
                    "BIL_ACTION_REQUEST_NOT_FOUND",
                    "Permintaan pembukaan kembali tidak ditemukan.");
            }

            if (actionRequest.ActionType != BillingFinancialActionType.FolioReopen)
            {
                return BillingServiceResult<FolioClosureResponse>.Validation(
                    "BIL_ACTION_TYPE_MISMATCH",
                    "Permintaan yang dirujuk bukan permintaan pembukaan kembali folio.");
            }

            if (actionRequest.FolioId != folio.Id)
            {
                return BillingServiceResult<FolioClosureResponse>.Validation(
                    "BIL_ACTION_FOLIO_MISMATCH",
                    "Permintaan yang dirujuk bukan milik folio ini.");
            }

            if (actionRequest.Status != BillingFinancialActionStatus.Approved)
            {
                return BillingServiceResult<FolioClosureResponse>.Conflict(
                    "BIL_ACTION_NOT_APPROVED",
                    $"Permintaan pembukaan kembali berstatus {actionRequest.Status}. Folio hanya " +
                    "dapat dibuka kembali atas permintaan yang sudah disetujui checker.");
            }

            var priorStatus = folio.Status;

            folio.Status = BillingFolioStatus.Open;
            folio.Version += 1;
            folio.UpdateBy = actorUserId;
            folio.UpdateDateTime = DateTime.UtcNow;

            actionRequest.Status = BillingFinancialActionStatus.Executed;
            actionRequest.ExecutedAt = DateTime.UtcNow;
            actionRequest.ExecutedByUserId = actorUserId;
            actionRequest.ExecutedAmount = actionRequest.RequestedAmount;
            actionRequest.UpdateBy = actorUserId;
            actionRequest.UpdateDateTime = DateTime.UtcNow;
            actionRequest.Version += 1;

            var history = new BilFolioClosureHistory
            {
                FolioId = folio.Id,
                EncounterId = folio.EncounterId,
                Action = BillingFolioClosureAction.Reopen,
                PriorStatus = priorStatus,
                NewStatus = BillingFolioStatus.Open,
                PerformedByUserId = actorUserId,
                PerformedAt = DateTime.UtcNow,
                Note = request.Note,
                FinancialActionRequestId = actionRequest.Id,
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };

            _dbContext.Set<BilFolioClosureHistory>().Add(history);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return BillingServiceResult<FolioClosureResponse>.Conflict(
                    "BIL_FOLIO_VERSION_CONFLICT",
                    "Folio berubah bersamaan dengan pembukaan ini. Muat ulang lalu ulangi.");
            }

            return BillingServiceResult<FolioClosureResponse>.Success(new FolioClosureResponse
            {
                FolioId = folio.Id,
                EncounterId = folio.EncounterId,
                Status = folio.Status,
                StatusName = folio.Status.ToString(),
                Version = folio.Version,
                PerformedAt = history.PerformedAt,
                PerformedByUserId = actorUserId,
                Message = "Folio dibuka kembali. Riwayat penutupan sebelumnya tetap tersimpan."
            });
        }

        public async Task<List<FolioClosureHistoryResponse>> GetHistoryAsync(
            Guid folioId,
            CancellationToken cancellationToken = default)
        {
            var items = await _dbContext.Set<BilFolioClosureHistory>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.FolioId == folioId)
                .OrderBy(x => x.PerformedAt)
                .ToListAsync(cancellationToken);

            return items.Select(x => new FolioClosureHistoryResponse
            {
                Id = x.Id,
                FolioId = x.FolioId,
                Action = x.Action,
                ActionName = x.Action.ToString(),
                PriorStatus = x.PriorStatus,
                NewStatus = x.NewStatus,
                PerformedByUserId = x.PerformedByUserId,
                PerformedAt = x.PerformedAt,
                Note = x.Note,
                FinancialActionRequestId = x.FinancialActionRequestId,
                ClosureEvidence = x.ClosureEvidence
            }).ToList();
        }

        private static string BuildClosureEvidence(
            FolioClosureReadinessResponse readiness,
            int unsettledActionCount) =>
            JsonSerializer.Serialize(new
            {
                EvaluatedAt = DateTime.UtcNow,
                readiness.CanClose,
                ReconciliationBlockerCount = readiness.Blockers.Count,
                UnsettledFinancialActionCount = unsettledActionCount
            });
    }
}
