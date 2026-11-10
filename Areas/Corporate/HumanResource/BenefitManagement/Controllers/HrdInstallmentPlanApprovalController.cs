using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Security;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Controllers;

/// <summary>
/// Persetujuan perjanjian angsuran piutang karyawan/dokter oleh HRD. Satu-satunya jalur menyetujui atau menolak:
/// Finance mengajukan (FinanceReceivableInstallmentPlan : Create), HRD memutuskan di sini. Logika keputusan, jadwal,
/// maker-checker, lock, dan sinkron payroll tetap dimiliki FinanceReceivableInstallmentPlanService — controller ini hanya
/// menegakkan permission HRD dan tidak menduplikasi aturan. Arketipe: worklist keputusan atas aggregate ber-lifecycle
/// (aksi lewat POST /{id}/&lt;aksi&gt;, bukan PATCH status generik).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/human-resource/benefit-management/installment-plan-approvals")]
[AccessController(
    moduleCode: "HUMAN_RESOURCE_BENEFIT_INSTALLMENT_PLAN_APPROVAL",
    moduleName: "Human Resource Benefit Installment Plan Approval",
    displayName: "HRD Installment Plan Approval",
    AreaName = "Corporate",
    ControllerName = "HrdInstallmentPlanApproval",
    Description = "Persetujuan perjanjian angsuran piutang karyawan/dokter oleh HRD",
    SortOrder = 16)]
[Tags("Corporate / Human Resource / Benefit Management / Installment Plan Approval")]
public sealed class HrdInstallmentPlanApprovalController : ControllerBase
{
    private const string ResourceName = "HrdInstallmentPlanApproval";
    private const string SelfApproveAction = "SelfApprove";

    private readonly FinanceReceivableInstallmentPlanService _service;
    private readonly FinanceInstallmentDeductionEvidenceService _evidenceService;
    private readonly AccessPermissionService _accessPermissionService;

    public HrdInstallmentPlanApprovalController(
        FinanceReceivableInstallmentPlanService service,
        FinanceInstallmentDeductionEvidenceService evidenceService,
        AccessPermissionService accessPermissionService)
    {
        _service = service;
        _evidenceService = evidenceService;
        _accessPermissionService = accessPermissionService;
    }

    [HttpGet]
    [AccessAction("Read", "Read Installment Plan Approvals", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("HrdInstallmentPlanApproval", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InstallmentPlanListResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] InstallmentPlanQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<InstallmentPlanListResponse>>.Ok(
            await _service.GetPagedAsync(request, cancellationToken), "Daftar permohonan perjanjian angsuran berhasil diambil."));

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Installment Plan Approval Detail", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("HrdInstallmentPlanApproval", "Read")]
    [ProducesResponseType(typeof(ApiResponse<InstallmentPlanDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetDetailAsync(id, cancellationToken);
            return Ok(ApiResponse<InstallmentPlanDetailResponse>.Ok(result, "Rincian permohonan perjanjian angsuran berhasil diambil."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    /// <summary>Menyetujui: jadwal angsuran terbit dan dikirim ke payroll HR. Tidak mengubah sisa piutang dan tidak memotong gaji.</summary>
    [HttpPost("{id:guid}/approve")]
    [AccessAction("Approve", "Approve Installment Plan", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("HrdInstallmentPlanApproval", "Approve")]
    [ProducesResponseType(typeof(ApiResponse<InstallmentPlanDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveInstallmentPlanRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ApproveAsync(id, request, CurrentUserId(), cancellationToken, await CanDecideOwnRequestAsync());
            return Ok(ApiResponse<InstallmentPlanDetailResponse>.Ok(result, "Perjanjian angsuran disetujui HRD dan jadwal angsuran diterbitkan."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/reject")]
    [AccessAction("Reject", "Reject Installment Plan", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("HrdInstallmentPlanApproval", "Reject")]
    [ProducesResponseType(typeof(ApiResponse<InstallmentPlanDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectInstallmentPlanRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.RejectAsync(id, request, CurrentUserId(), cancellationToken, await CanDecideOwnRequestAsync());
            return Ok(ApiResponse<InstallmentPlanDetailResponse>.Ok(result, "Perjanjian angsuran ditolak HRD."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    /// <summary>
    /// Pemulihan: mengirim ulang angsuran DIJADWALKAN ke payroll HR bila saat persetujuan payroll belum dapat menerimanya.
    /// Idempoten (upsert per angsuran) dan tidak mengubah status, jadwal, maupun saldo.
    /// </summary>
    [HttpPost("{id:guid}/payroll-sync")]
    [AccessAction("SyncPayroll", "Retry Installment Plan Payroll Sync", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("HrdInstallmentPlanApproval", "SyncPayroll")]
    [ProducesResponseType(typeof(ApiResponse<InstallmentPayrollSyncResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RetryPayrollSync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.RetryPayrollSyncAsync(id, CurrentUserId(), cancellationToken);
            var message = result.SkippedCount > 0
                ? "Sinkron payroll dijalankan, tetapi sebagian angsuran belum dapat diterima payroll."
                : "Jadwal angsuran berhasil dikirim ke payroll.";
            return Ok(ApiResponse<InstallmentPayrollSyncResultResponse>.Ok(result, message));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    // ------------------------------------------------------------------------------------
    // Bukti potong gaji. HRD menyimpan dan memverifikasi; Finance membaca dari sumber yang sama.
    // Tidak satu pun endpoint di bawah ini mengubah saldo piutang atau status angsuran: pengurangan piutang hanya lewat
    // hasil payroll/batch yang sah (FinanceReceivablePayrollSyncService.ApplyResultCoreAsync).
    // ------------------------------------------------------------------------------------

    [HttpGet("{id:guid}/deduction-evidence")]
    [AccessAction("Read", "Read Installment Plan Approval Detail", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("HrdInstallmentPlanApproval", "Read")]
    [ProducesResponseType(typeof(ApiResponse<InstallmentDeductionEvidenceOverviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDeductionEvidence(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _evidenceService.GetOverviewAsync(id, cancellationToken);
            return Ok(ApiResponse<InstallmentDeductionEvidenceOverviewResponse>.Ok(result, "Bukti potong gaji perjanjian angsuran berhasil diambil."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpGet("{id:guid}/deduction-evidence/{proofId:guid}/file")]
    [AccessAction("ReadDeductionProof", "Read Installment Deduction Proof File", AccessType = AccessTypes.Read, SortOrder = 6)]
    [AccessPermission("HrdInstallmentPlanApproval", "ReadDeductionProof")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadDeductionEvidence(Guid id, Guid proofId, CancellationToken cancellationToken)
    {
        try
        {
            var (stream, mediaType, fileName) = await _evidenceService.OpenFileAsync(id, proofId, CurrentUserId(), cancellationToken);
            return File(stream, mediaType, fileName);
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    /// <summary>Menyimpan bukti potong gaji satu angsuran (multipart). Tidak mengubah saldo piutang maupun status angsuran.</summary>
    [HttpPost("{id:guid}/installments/{installmentId:guid}/deduction-evidence")]
    [AccessAction("UploadDeductionProof", "Upload Installment Deduction Proof", AccessType = AccessTypes.Create, SortOrder = 7)]
    [AccessPermission("HrdInstallmentPlanApproval", "UploadDeductionProof")]
    [ProducesResponseType(typeof(ApiResponse<InstallmentDeductionEvidenceOverviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UploadDeductionEvidence(
        Guid id, Guid installmentId, [FromForm] UploadInstallmentDeductionEvidenceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _evidenceService.UploadAsync(id, installmentId, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<InstallmentDeductionEvidenceOverviewResponse>.Ok(result, "Bukti potong gaji berhasil disimpan. Saldo piutang tidak berubah."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/deduction-evidence/{evidenceId:guid}/verify")]
    [AccessAction("VerifyDeductionProof", "Verify Installment Deduction Proof", AccessType = AccessTypes.Update, SortOrder = 8)]
    [AccessPermission("HrdInstallmentPlanApproval", "VerifyDeductionProof")]
    [ProducesResponseType(typeof(ApiResponse<InstallmentDeductionEvidenceOverviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> VerifyDeductionEvidence(
        Guid id, Guid evidenceId, [FromBody] VerifyInstallmentDeductionEvidenceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _evidenceService.VerifyAsync(id, evidenceId, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<InstallmentDeductionEvidenceOverviewResponse>.Ok(result, "Bukti potong gaji diverifikasi. Saldo piutang tidak berubah."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/deduction-evidence/{evidenceId:guid}/reject")]
    [AccessAction("VerifyDeductionProof", "Verify Installment Deduction Proof", AccessType = AccessTypes.Update, SortOrder = 8)]
    [AccessPermission("HrdInstallmentPlanApproval", "VerifyDeductionProof")]
    [ProducesResponseType(typeof(ApiResponse<InstallmentDeductionEvidenceOverviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RejectDeductionEvidence(
        Guid id, Guid evidenceId, [FromBody] RejectInstallmentDeductionEvidenceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _evidenceService.RejectAsync(id, evidenceId, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<InstallmentDeductionEvidenceOverviewResponse>.Ok(result, "Bukti potong gaji ditolak. Saldo piutang tidak berubah."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(404, exception.Message)),
        InstallmentPlanForbiddenException => StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(403, exception.Message)),
        InstallmentPlanConflictException => Conflict(ApiResponse<object>.Fail(409, exception.Message)),
        InstallmentPlanValidationException => UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)),
        FinanceTransactionProofValidationException => BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, exception.Message)),
        FinanceTransactionProofTooLargeException => StatusCode(StatusCodes.Status413PayloadTooLarge,
            ApiResponse<object>.Fail(StatusCodes.Status413PayloadTooLarge, exception.Message)),
        FinanceTransactionProofNotConfiguredException => StatusCode(StatusCodes.Status503ServiceUnavailable,
            ApiResponse<object>.Fail(StatusCodes.Status503ServiceUnavailable, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) =>
        exception is KeyNotFoundException or InstallmentPlanConflictException or InstallmentPlanValidationException
            or InstallmentPlanForbiddenException or FinanceTransactionProofValidationException
            or FinanceTransactionProofTooLargeException or FinanceTransactionProofNotConfiguredException;

    // Satu sumber kebenaran izin: Akses Role. Controller hanya menjawab "apakah pemanggil memegang SelfApprove";
    // aturan pemakaiannya (pengaju == pemutus, kesiapan skema) tetap di service agar satu kebijakan untuk setujui dan tolak.
    private Task<bool> CanDecideOwnRequestAsync() =>
        _accessPermissionService.HasAccessAsync(User, ResourceName, SelfApproveAction);

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
