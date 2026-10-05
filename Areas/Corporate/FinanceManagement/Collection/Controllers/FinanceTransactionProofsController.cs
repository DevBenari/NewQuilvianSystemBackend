using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Controllers;

/// <summary>
/// Unggah dan pembacaan metadata bukti pembayaran langsung (BE-FIN-075, FIN-API-1.6 F.3,
/// FIN-DES-092). Resource dan atribut controller persis seperti yang sudah dicatat
/// `permission-audit-matrix.md` G.1 sejak revisi 14 — nol resource/action baru pada task ini.
///
/// Nol endpoint PUT/DELETE (FIN-DEC-139): bukti tidak dapat diganti sesudah baris mutasi tertulis.
/// Koreksi berjalan lewat pembalikan pembayaran, bukan penggantian bukti.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/transaction-proofs")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_TRANSACTION_PROOF",
    "Corporate Finance Management Transaction Proof", "Transaction Proof",
    AreaName = "Corporate", ControllerName = "FinanceTransactionProof",
    Description = "Bukti pembayaran langsung piutang dan utang", SortOrder = 72)]
[Tags("Corporate / Finance Management / Transaction Proof")]
public sealed class FinanceTransactionProofsController : ControllerBase
{
    private readonly FinanceTransactionProofService _service;

    public FinanceTransactionProofsController(FinanceTransactionProofService service)
    {
        _service = service;
    }

    /// <summary>
    /// Mengunggah bukti pembayaran lewat delapan pemeriksaan berurut (FIN-DES-092 M.3). Mengembalikan
    /// ProofId untuk dipakai pada pembayaran langsung piutang/utang (BE-FIN-077/078).
    /// </summary>
    [HttpPost]
    [AccessAction("Create", "Create Transaction Proof", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("FinanceTransactionProof", "Create")]
    [ProducesResponseType(typeof(ApiResponse<TransactionProofResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Upload([FromForm] UploadTransactionProofRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _service.UploadAsync(request.File, request.ProofType, CurrentUserId(), cancellationToken);
            var result = await _service.GetMetadataAsync(entity.Id, cancellationToken);
            return Ok(ApiResponse<TransactionProofResponse>.Ok(result, "Bukti pembayaran berhasil diunggah."));
        }
        catch (FinanceTransactionProofValidationException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, exception.Message));
        }
        catch (FinanceTransactionProofTooLargeException exception)
        {
            return StatusCode(StatusCodes.Status413PayloadTooLarge,
                ApiResponse<object>.Fail(StatusCodes.Status413PayloadTooLarge, exception.Message));
        }
        catch (FinanceTransactionProofNotConfiguredException exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                ApiResponse<object>.Fail(StatusCodes.Status503ServiceUnavailable, exception.Message));
        }
    }

    /// <summary>Mengunduh berkas bukti. Bergerbang Read tanpa pembatasan per pemilik transaksi (permission-audit-matrix.md H.3).</summary>
    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Transaction Proof", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceTransactionProof", "Read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var (stream, mediaType, fileName) = await _service.DownloadAsync(id, CurrentUserId(), cancellationToken);
            return File(stream, mediaType, fileName);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
        }
    }

    /// <summary>Keterangan berkas bukti tanpa mengunduh isinya.</summary>
    [HttpGet("{id:guid}/metadata")]
    [AccessAction("Read", "Read Transaction Proof", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceTransactionProof", "Read")]
    [ProducesResponseType(typeof(ApiResponse<TransactionProofResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMetadata(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetMetadataAsync(id, cancellationToken);
            return Ok(ApiResponse<TransactionProofResponse>.Ok(result, "Metadata bukti pembayaran berhasil diambil."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
        }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
