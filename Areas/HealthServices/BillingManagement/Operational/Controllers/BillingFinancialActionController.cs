using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Controllers
{
    /// <summary>
    /// Permukaan HTTP tindakan finansial, persetujuan, dan penutupan folio —
    /// <c>RJ-BIL-BE-006</c>, melaksanakan <c>RJ-BIL-GATE-DEC-006</c>.
    ///
    /// <para><b>Mengapa endpoint-nya banyak dan bukan satu yang serba bisa.</b></para>
    ///
    /// <c>RJ-BIL-GATE-DEC-006</c> butir <c>1</c> menuntut kemampuan yang terpisah untuk
    /// void, adjustment, reversal, refund, waiver, write-off, manual override, serta close dan
    /// reopen folio. Kewenangan di sistem ini diberikan per pasangan controller dan action, jadi
    /// satu endpoint serba bisa berarti satu kewenangan serba bisa — dan orang yang hanya boleh
    /// mengajukan adjustment diam-diam menjadi boleh mengajukan refund.
    ///
    /// Karena itu pengajuan dan persetujuan dipecah per jenis. Jenis tindakan diambil dari
    /// <b>rute</b>, bukan dari isi permintaan, sehingga pengirim tidak dapat melewati gerbang
    /// kewenangan dengan menukar satu field di body.
    ///
    /// Yang tidak dipecah adalah submit, revise, dan cancel: ketiganya hanya boleh dilakukan
    /// pengaju permintaan itu sendiri, yang sudah harus memegang kewenangan pengajuan jenis
    /// tersebut sejak awal.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/billing-management/financial-actions")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_BILLING_MANAGEMENT",
        moduleName: "Health Service Billing Management",
        displayName: "Billing Financial Action",
        AreaName = "HealthServices",
        ControllerName = "BillingFinancialAction",
        Description = "Tindakan finansial, maker-checker, dan penutupan folio Billing",
        SortOrder = 3)]
    [Tags("Health Services / Billing Management / Billing Financial Action")]
    public class BillingFinancialActionController : ControllerBase
    {
        private const string LogCategory = "HealthServices.BillingManagement";

        private readonly BillingFinancialActionService _actionService;
        private readonly BillingFolioClosureService _closureService;
        private readonly LoggerService _loggerService;

        public BillingFinancialActionController(
            BillingFinancialActionService actionService,
            BillingFolioClosureService closureService,
            LoggerService loggerService)
        {
            _actionService = actionService;
            _closureService = closureService;
            _loggerService = loggerService;
        }

        // =================================================================
        // Pembacaan
        // =================================================================

        [HttpGet("requests")]
        [ProducesResponseType(typeof(ApiResponse<List<FinancialActionRequestResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Financial Action", Description = "Melihat daftar permintaan tindakan finansial", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BillingFinancialAction", "Read")]
        public async Task<IActionResult> GetRequests(
            [FromQuery] Guid? folioId,
            [FromQuery] Guid? encounterId,
            [FromQuery] BillingFinancialActionType? actionType,
            [FromQuery] BillingFinancialActionStatus? status,
            CancellationToken cancellationToken = default)
        {
            var items = await _actionService.GetAsync(
                folioId, encounterId, actionType, status, cancellationToken);

            return Ok(ApiResponse<List<FinancialActionRequestResponse>>.Ok(
                items,
                "Daftar permintaan tindakan finansial berhasil diambil."));
        }

        [HttpGet("requests/{requestId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FinancialActionRequestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Financial Action", Description = "Melihat satu permintaan tindakan finansial", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("BillingFinancialAction", "Read")]
        public async Task<IActionResult> GetRequestById(
            Guid requestId,
            CancellationToken cancellationToken = default)
        {
            var item = await _actionService.GetByIdAsync(requestId, cancellationToken);

            if (item == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Permintaan tindakan finansial tidak ditemukan.",
                    new { Code = "BIL_ACTION_REQUEST_NOT_FOUND" }));
            }

            return Ok(ApiResponse<FinancialActionRequestResponse>.Ok(
                item,
                "Permintaan tindakan finansial berhasil diambil."));
        }

        // =================================================================
        // Pengajuan — satu endpoint per jenis, satu kewenangan per jenis
        // =================================================================

        [HttpPost("requests/void")]
        [ProducesResponseType(typeof(ApiResponse<FinancialActionRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction("VoidCreate", "Create Void Request", Description = "Mengajukan pembatalan tagihan", AccessType = AccessTypes.Create, SortOrder = 10)]
        [AccessPermission("BillingFinancialAction", "VoidCreate")]
        public Task<IActionResult> CreateVoid(
            [FromBody] CreateFinancialActionRequest request,
            CancellationToken cancellationToken = default) =>
            CreateAsync(BillingFinancialActionType.Void, request, cancellationToken);

        [HttpPost("requests/adjustment")]
        [ProducesResponseType(typeof(ApiResponse<FinancialActionRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction("AdjustmentCreate", "Create Adjustment Request", Description = "Mengajukan koreksi nilai tagihan", AccessType = AccessTypes.Create, SortOrder = 11)]
        [AccessPermission("BillingFinancialAction", "AdjustmentCreate")]
        public Task<IActionResult> CreateAdjustment(
            [FromBody] CreateFinancialActionRequest request,
            CancellationToken cancellationToken = default) =>
            CreateAsync(BillingFinancialActionType.Adjustment, request, cancellationToken);

        [HttpPost("requests/reversal")]
        [ProducesResponseType(typeof(ApiResponse<FinancialActionRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction("ReversalCreate", "Create Reversal Request", Description = "Mengajukan pembalikan tagihan", AccessType = AccessTypes.Create, SortOrder = 12)]
        [AccessPermission("BillingFinancialAction", "ReversalCreate")]
        public Task<IActionResult> CreateReversal(
            [FromBody] CreateFinancialActionRequest request,
            CancellationToken cancellationToken = default) =>
            CreateAsync(BillingFinancialActionType.Reversal, request, cancellationToken);

        [HttpPost("requests/refund")]
        [ProducesResponseType(typeof(ApiResponse<FinancialActionRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction("RefundCreate", "Create Refund Request", Description = "Mengajukan pengembalian dana", AccessType = AccessTypes.Create, SortOrder = 13)]
        [AccessPermission("BillingFinancialAction", "RefundCreate")]
        public Task<IActionResult> CreateRefund(
            [FromBody] CreateFinancialActionRequest request,
            CancellationToken cancellationToken = default) =>
            CreateAsync(BillingFinancialActionType.Refund, request, cancellationToken);

        [HttpPost("requests/waiver")]
        [ProducesResponseType(typeof(ApiResponse<FinancialActionRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction("WaiverCreate", "Create Waiver Request", Description = "Mengajukan pembebasan biaya (FOC)", AccessType = AccessTypes.Create, SortOrder = 14)]
        [AccessPermission("BillingFinancialAction", "WaiverCreate")]
        public Task<IActionResult> CreateWaiver(
            [FromBody] CreateFinancialActionRequest request,
            CancellationToken cancellationToken = default) =>
            CreateAsync(BillingFinancialActionType.Waiver, request, cancellationToken);

        [HttpPost("requests/write-off")]
        [ProducesResponseType(typeof(ApiResponse<FinancialActionRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction("WriteOffCreate", "Create Write Off Request", Description = "Mengajukan penghapusan piutang", AccessType = AccessTypes.Create, SortOrder = 15)]
        [AccessPermission("BillingFinancialAction", "WriteOffCreate")]
        public Task<IActionResult> CreateWriteOff(
            [FromBody] CreateFinancialActionRequest request,
            CancellationToken cancellationToken = default) =>
            CreateAsync(BillingFinancialActionType.WriteOff, request, cancellationToken);

        [HttpPost("requests/manual-override")]
        [ProducesResponseType(typeof(ApiResponse<FinancialActionRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction("ManualOverrideCreate", "Create Manual Override Request", Description = "Mengajukan override manual", AccessType = AccessTypes.Create, SortOrder = 16)]
        [AccessPermission("BillingFinancialAction", "ManualOverrideCreate")]
        public Task<IActionResult> CreateManualOverride(
            [FromBody] CreateFinancialActionRequest request,
            CancellationToken cancellationToken = default) =>
            CreateAsync(BillingFinancialActionType.ManualOverride, request, cancellationToken);

        [HttpPost("requests/folio-reopen")]
        [ProducesResponseType(typeof(ApiResponse<FinancialActionRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction("FolioReopenCreate", "Create Folio Reopen Request", Description = "Mengajukan pembukaan kembali folio", AccessType = AccessTypes.Create, SortOrder = 17)]
        [AccessPermission("BillingFinancialAction", "FolioReopenCreate")]
        public Task<IActionResult> CreateFolioReopen(
            [FromBody] CreateFinancialActionRequest request,
            CancellationToken cancellationToken = default) =>
            CreateAsync(BillingFinancialActionType.FolioReopen, request, cancellationToken);

        // =================================================================
        // Milik pengaju sendiri
        // =================================================================

        [HttpPost("requests/{requestId:guid}/submit")]
        [ProducesResponseType(typeof(ApiResponse<FinancialActionRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction("Submit", "Submit Financial Action", Description = "Mengajukan permintaan untuk diputuskan", AccessType = AccessTypes.Update, SortOrder = 20)]
        [AccessPermission("BillingFinancialAction", "Submit")]
        public async Task<IActionResult> Submit(
            Guid requestId,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty) return UnauthorizedActor();

            var result = await _actionService.SubmitAsync(requestId, actorUserId, cancellationToken);

            return MapResult(result, "Permintaan berhasil diajukan.");
        }

        [HttpPost("requests/{requestId:guid}/revise")]
        [ProducesResponseType(typeof(ApiResponse<FinancialActionRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction("Revise", "Revise Financial Action", Description = "Menerbitkan revisi permintaan", AccessType = AccessTypes.Update, SortOrder = 21)]
        [AccessPermission("BillingFinancialAction", "Revise")]
        public async Task<IActionResult> Revise(
            Guid requestId,
            [FromBody] ReviseFinancialActionRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty) return UnauthorizedActor();

            var result = await _actionService.ReviseAsync(
                requestId, request, actorUserId, cancellationToken);

            return MapResult(result, "Revisi permintaan berhasil diterbitkan.");
        }

        [HttpPost("requests/{requestId:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<FinancialActionRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction("Cancel", "Cancel Financial Action", Description = "Membatalkan permintaan yang belum dijalankan", AccessType = AccessTypes.Update, SortOrder = 22)]
        [AccessPermission("BillingFinancialAction", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid requestId,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty) return UnauthorizedActor();

            var result = await _actionService.CancelAsync(requestId, actorUserId, cancellationToken);

            return MapResult(result, "Permintaan berhasil dibatalkan.");
        }

        // =================================================================
        // Keputusan checker — satu endpoint per jenis, satu kewenangan per jenis
        // =================================================================

        [HttpPost("requests/{requestId:guid}/decide/void")]
        [AccessAction("VoidApprove", "Approve Void Request", Description = "Memutuskan permintaan pembatalan tagihan", AccessType = AccessTypes.Update, SortOrder = 30)]
        [AccessPermission("BillingFinancialAction", "VoidApprove")]
        public Task<IActionResult> DecideVoid(Guid requestId, [FromBody] DecideFinancialActionRequest request, CancellationToken cancellationToken = default) =>
            DecideAsync(BillingFinancialActionType.Void, requestId, request, cancellationToken);

        [HttpPost("requests/{requestId:guid}/decide/adjustment")]
        [AccessAction("AdjustmentApprove", "Approve Adjustment Request", Description = "Memutuskan permintaan koreksi nilai tagihan", AccessType = AccessTypes.Update, SortOrder = 31)]
        [AccessPermission("BillingFinancialAction", "AdjustmentApprove")]
        public Task<IActionResult> DecideAdjustment(Guid requestId, [FromBody] DecideFinancialActionRequest request, CancellationToken cancellationToken = default) =>
            DecideAsync(BillingFinancialActionType.Adjustment, requestId, request, cancellationToken);

        [HttpPost("requests/{requestId:guid}/decide/reversal")]
        [AccessAction("ReversalApprove", "Approve Reversal Request", Description = "Memutuskan permintaan pembalikan tagihan", AccessType = AccessTypes.Update, SortOrder = 32)]
        [AccessPermission("BillingFinancialAction", "ReversalApprove")]
        public Task<IActionResult> DecideReversal(Guid requestId, [FromBody] DecideFinancialActionRequest request, CancellationToken cancellationToken = default) =>
            DecideAsync(BillingFinancialActionType.Reversal, requestId, request, cancellationToken);

        [HttpPost("requests/{requestId:guid}/decide/refund")]
        [AccessAction("RefundApprove", "Approve Refund Request", Description = "Memutuskan permintaan pengembalian dana", AccessType = AccessTypes.Update, SortOrder = 33)]
        [AccessPermission("BillingFinancialAction", "RefundApprove")]
        public Task<IActionResult> DecideRefund(Guid requestId, [FromBody] DecideFinancialActionRequest request, CancellationToken cancellationToken = default) =>
            DecideAsync(BillingFinancialActionType.Refund, requestId, request, cancellationToken);

        [HttpPost("requests/{requestId:guid}/decide/waiver")]
        [AccessAction("WaiverApprove", "Approve Waiver Request", Description = "Memutuskan permintaan pembebasan biaya", AccessType = AccessTypes.Update, SortOrder = 34)]
        [AccessPermission("BillingFinancialAction", "WaiverApprove")]
        public Task<IActionResult> DecideWaiver(Guid requestId, [FromBody] DecideFinancialActionRequest request, CancellationToken cancellationToken = default) =>
            DecideAsync(BillingFinancialActionType.Waiver, requestId, request, cancellationToken);

        [HttpPost("requests/{requestId:guid}/decide/write-off")]
        [AccessAction("WriteOffApprove", "Approve Write Off Request", Description = "Memutuskan permintaan penghapusan piutang", AccessType = AccessTypes.Update, SortOrder = 35)]
        [AccessPermission("BillingFinancialAction", "WriteOffApprove")]
        public Task<IActionResult> DecideWriteOff(Guid requestId, [FromBody] DecideFinancialActionRequest request, CancellationToken cancellationToken = default) =>
            DecideAsync(BillingFinancialActionType.WriteOff, requestId, request, cancellationToken);

        [HttpPost("requests/{requestId:guid}/decide/manual-override")]
        [AccessAction("ManualOverrideApprove", "Approve Manual Override Request", Description = "Memutuskan permintaan override manual", AccessType = AccessTypes.Update, SortOrder = 36)]
        [AccessPermission("BillingFinancialAction", "ManualOverrideApprove")]
        public Task<IActionResult> DecideManualOverride(Guid requestId, [FromBody] DecideFinancialActionRequest request, CancellationToken cancellationToken = default) =>
            DecideAsync(BillingFinancialActionType.ManualOverride, requestId, request, cancellationToken);

        [HttpPost("requests/{requestId:guid}/decide/folio-reopen")]
        [AccessAction("FolioReopenApprove", "Approve Folio Reopen Request", Description = "Memutuskan permintaan pembukaan kembali folio", AccessType = AccessTypes.Update, SortOrder = 37)]
        [AccessPermission("BillingFinancialAction", "FolioReopenApprove")]
        public Task<IActionResult> DecideFolioReopen(Guid requestId, [FromBody] DecideFinancialActionRequest request, CancellationToken cancellationToken = default) =>
            DecideAsync(BillingFinancialActionType.FolioReopen, requestId, request, cancellationToken);

        // =================================================================
        // Pelaksanaan
        // =================================================================

        /// <summary>
        /// Menjalankan tindakan yang sudah disetujui, kecuali refund.
        ///
        /// Refund punya endpoint tersendiri karena <c>RJ-BIL-GATE-DEC-006</c> memisahkan
        /// kemampuan <i>execute</i>-nya. Ia satu-satunya tindakan yang mengeluarkan uang dari
        /// rumah sakit, sehingga menyetujuinya dan benar-benar menjalankannya sengaja dijadikan
        /// dua kewenangan berbeda.
        /// </summary>
        [HttpPost("requests/{requestId:guid}/execute")]
        [AccessAction("Execute", "Execute Financial Action", Description = "Menjalankan tindakan finansial yang sudah disetujui", AccessType = AccessTypes.Update, SortOrder = 40)]
        [AccessPermission("BillingFinancialAction", "Execute")]
        public async Task<IActionResult> Execute(
            Guid requestId,
            [FromBody] ExecuteFinancialActionRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty) return UnauthorizedActor();

            var existing = await _actionService.GetByIdAsync(requestId, cancellationToken);

            if (existing != null && existing.ActionType == BillingFinancialActionType.Refund)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Refund dijalankan melalui endpoint refund tersendiri, karena kewenangan " +
                    "menjalankannya dipisahkan dari kewenangan menyetujuinya.",
                    new { Code = "BIL_USE_REFUND_EXECUTE_ENDPOINT" }));
            }

            return await ExecuteInternalAsync(requestId, request, actorUserId, cancellationToken);
        }

        [HttpPost("requests/{requestId:guid}/execute-refund")]
        [AccessAction("RefundExecute", "Execute Refund", Description = "Menjalankan pengembalian dana yang sudah disetujui", AccessType = AccessTypes.Update, SortOrder = 41)]
        [AccessPermission("BillingFinancialAction", "RefundExecute")]
        public async Task<IActionResult> ExecuteRefund(
            Guid requestId,
            [FromBody] ExecuteFinancialActionRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty) return UnauthorizedActor();

            var existing = await _actionService.GetByIdAsync(requestId, cancellationToken);

            if (existing != null && existing.ActionType != BillingFinancialActionType.Refund)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Permintaan ini bukan refund.",
                    new { Code = "BIL_ACTION_TYPE_MISMATCH" }));
            }

            return await ExecuteInternalAsync(requestId, request, actorUserId, cancellationToken);
        }

        // =================================================================
        // Penutupan folio
        // =================================================================

        [HttpPost("folios/{folioId:guid}/close")]
        [ProducesResponseType(typeof(ApiResponse<FolioClosureResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("FolioClose", "Close Folio", Description = "Menutup folio bila tidak ada lagi yang menahannya", AccessType = AccessTypes.Update, SortOrder = 50)]
        [AccessPermission("BillingFinancialAction", "FolioClose")]
        public async Task<IActionResult> CloseFolio(
            Guid folioId,
            [FromBody] CloseFolioRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty) return UnauthorizedActor();

            var result = await _closureService.CloseAsync(
                folioId, request, actorUserId, cancellationToken);

            if (result.Kind == BillingServiceResultKind.Success)
            {
                await _loggerService.AuditAsync(
                    LogCategory,
                    "BillingFinancialAction.CloseFolio",
                    "Menutup folio Billing.",
                    new { UserId = actorUserId, FolioId = folioId });
            }

            return MapResult(result, "Folio berhasil ditutup.");
        }

        [HttpPost("folios/{folioId:guid}/reopen")]
        [ProducesResponseType(typeof(ApiResponse<FolioClosureResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("FolioReopenExecute", "Reopen Folio", Description = "Membuka kembali folio atas permintaan yang sudah disetujui", AccessType = AccessTypes.Update, SortOrder = 51)]
        [AccessPermission("BillingFinancialAction", "FolioReopenExecute")]
        public async Task<IActionResult> ReopenFolio(
            Guid folioId,
            [FromBody] ReopenFolioRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty) return UnauthorizedActor();

            var result = await _closureService.ReopenAsync(
                folioId, request, actorUserId, cancellationToken);

            if (result.Kind == BillingServiceResultKind.Success)
            {
                await _loggerService.AuditAsync(
                    LogCategory,
                    "BillingFinancialAction.ReopenFolio",
                    "Membuka kembali folio Billing yang sudah tertutup.",
                    new
                    {
                        UserId = actorUserId,
                        FolioId = folioId,
                        request.FinancialActionRequestId
                    });
            }

            return MapResult(result, "Folio berhasil dibuka kembali.");
        }

        [HttpGet("folios/{folioId:guid}/closure-history")]
        [ProducesResponseType(typeof(ApiResponse<List<FolioClosureHistoryResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Folio Closure History", Description = "Melihat riwayat penutupan folio", AccessType = AccessTypes.Read, SortOrder = 52)]
        [AccessPermission("BillingFinancialAction", "Read")]
        public async Task<IActionResult> GetClosureHistory(
            Guid folioId,
            CancellationToken cancellationToken = default)
        {
            var items = await _closureService.GetHistoryAsync(folioId, cancellationToken);

            return Ok(ApiResponse<List<FolioClosureHistoryResponse>>.Ok(
                items,
                "Riwayat penutupan folio berhasil diambil."));
        }

        // =================================================================
        // Pembantu
        // =================================================================

        private async Task<IActionResult> CreateAsync(
            BillingFinancialActionType actionType,
            CreateFinancialActionRequest request,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty) return UnauthorizedActor();

            // Jenis tindakan diambil dari rute, menimpa apa pun yang dikirim pada body.
            // Gerbang kewenangan menjaga rute; membiarkan body menentukan jenis akan membuat
            // gerbang itu dapat dilewati dengan menukar satu field.
            request.ActionType = actionType;

            var result = await _actionService.CreateAsync(request, actorUserId, cancellationToken);

            return MapResult(result, "Permintaan tindakan finansial berhasil dibuat.");
        }

        private async Task<IActionResult> DecideAsync(
            BillingFinancialActionType actionType,
            Guid requestId,
            DecideFinancialActionRequest request,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty) return UnauthorizedActor();

            var existing = await _actionService.GetByIdAsync(requestId, cancellationToken);

            if (existing == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Permintaan tindakan finansial tidak ditemukan.",
                    new { Code = "BIL_ACTION_REQUEST_NOT_FOUND" }));
            }

            // Kewenangan yang dijaga rute harus benar-benar sepadan dengan jenis permintaan yang
            // dituju. Tanpa pemeriksaan ini, pemegang kewenangan menyetujui adjustment dapat
            // menyetujui refund lewat rute adjustment.
            if (existing.ActionType != actionType)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Permintaan ini berjenis {existing.ActionType}, bukan {actionType}. " +
                    "Gunakan rute yang sesuai dengan jenisnya.",
                    new { Code = "BIL_ACTION_TYPE_MISMATCH" }));
            }

            var result = await _actionService.DecideAsync(
                requestId, request, actorUserId, cancellationToken);

            if (result.Kind == BillingServiceResultKind.Success)
            {
                await _loggerService.AuditAsync(
                    LogCategory,
                    "BillingFinancialAction.Decide",
                    "Memutuskan permintaan tindakan finansial.",
                    new
                    {
                        UserId = actorUserId,
                        RequestId = requestId,
                        ActionType = actionType,
                        request.Decision,
                        request.ApprovedAmount
                    });
            }

            return MapResult(result, "Keputusan berhasil dicatat.");
        }

        private async Task<IActionResult> ExecuteInternalAsync(
            Guid requestId,
            ExecuteFinancialActionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var result = await _actionService.ExecuteAsync(
                requestId, request, actorUserId, cancellationToken);

            if (result.Kind == BillingServiceResultKind.Success)
            {
                await _loggerService.AuditAsync(
                    LogCategory,
                    "BillingFinancialAction.Execute",
                    "Menjalankan tindakan finansial yang sudah disetujui.",
                    new
                    {
                        UserId = actorUserId,
                        RequestId = requestId,
                        ActionType = result.Value?.ActionType,
                        ExecutedAmount = result.Value?.ExecutedAmount
                    });
            }

            return MapResult(result, "Tindakan finansial berhasil dijalankan.");
        }

        private IActionResult MapResult<T>(BillingServiceResult<T> result, string successMessage)
        {
            if (result.Kind == BillingServiceResultKind.Success && result.Value != null)
            {
                return Ok(ApiResponse<T>.Ok(result.Value, successMessage));
            }

            return result.Kind switch
            {
                BillingServiceResultKind.NotFound => NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    result.ErrorMessage ?? "Data tidak ditemukan.",
                    new { Code = result.ErrorCode })),

                BillingServiceResultKind.Conflict => Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict,
                    result.ErrorMessage ?? "Terjadi konflik.",
                    new { Code = result.ErrorCode })),

                _ => BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    result.ErrorMessage ?? "Permintaan tidak valid.",
                    new { Code = result.ErrorCode }))
            };
        }

        private IActionResult UnauthorizedActor() =>
            Unauthorized(ApiResponse<object>.Fail(
                StatusCodes.Status401Unauthorized,
                "Identitas actor tidak tersedia.",
                new { Code = "BIL_FORBIDDEN" }));

        private Guid GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdValue, out var userId) ? userId : Guid.Empty;
        }
    }
}
