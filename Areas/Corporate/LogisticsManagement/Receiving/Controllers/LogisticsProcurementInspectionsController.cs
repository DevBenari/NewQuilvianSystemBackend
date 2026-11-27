using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.LogisticsManagement.Receiving.Dtos;
using QuilvianSystemBackend.Areas.Corporate.LogisticsManagement.Receiving.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.LogisticsManagement.Receiving.Controllers;

/// <summary>
/// LGS-BE-REC-INSP-001 — Pemeriksaan Hasil Pengadaan (Logistik). Kontrak BACA saja: tidak ada aksi tulis, tidak ada perubahan status
/// dokumen, tidak ada posting stok. Izin memakai resource Logistik tersendiri (<c>LogisticsProcurementInspection</c>), sehingga petugas
/// Logistik tidak memerlukan <c>FinanceGoodsReceipt:Read</c>; data sumber tetap milik Finance dan dibaca internal oleh service.
/// Pencatatan hasil pemeriksaan sengaja BELUM ada (LGS-OQ-015/017/018 terbuka; memerlukan skema baru).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/logistics-management/receiving/procurement-inspections")]
[AccessController("CORPORATE_LOGISTICS_MANAGEMENT_RECEIVING", "Corporate Logistics Management Receiving", "Procurement Inspection",
    AreaName = "Corporate", ControllerName = "LogisticsProcurementInspection",
    Description = "Pemeriksaan Hasil Pengadaan Logistik (baca-saja)", SortOrder = 60)]
[Tags("Corporate / Logistics Management / Receiving / Procurement Inspection")]
public sealed class LogisticsProcurementInspectionsController : ControllerBase
{
    private readonly LogisticsProcurementInspectionQueryService _service;

    public LogisticsProcurementInspectionsController(LogisticsProcurementInspectionQueryService service) => _service = service;

    /// <summary>Daftar baris pemeriksaan (satu baris = satu baris Goods Receipt), berpaging.</summary>
    [HttpGet]
    [AccessAction("Read", "Read Procurement Inspection", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("LogisticsProcurementInspection", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProcurementInspectionItemResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetList([FromQuery] ProcurementInspectionQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetPagedAsync(query, cancellationToken);
            return Ok(ApiResponse<PagedResult<ProcurementInspectionItemResponse>>.Ok(result, "Daftar pemeriksaan hasil pengadaan berhasil diambil."));
        }
        catch (LogisticsReceivingValidationException exception) { return BadRequest(ApiResponse<object>.Fail(400, exception.Message)); }
    }

    /// <summary>Ringkasan jumlah dokumen/item menurut saringan yang sama dengan daftar. Hitungan, bukan jumlah kuantitas.</summary>
    [HttpGet("summary")]
    [AccessAction("Read", "Read Procurement Inspection", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("LogisticsProcurementInspection", "Read")]
    [ProducesResponseType(typeof(ApiResponse<ProcurementInspectionSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSummary([FromQuery] ProcurementInspectionQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetSummaryAsync(query, cancellationToken);
            return Ok(ApiResponse<ProcurementInspectionSummaryResponse>.Ok(result, "Ringkasan pemeriksaan hasil pengadaan berhasil diambil."));
        }
        catch (LogisticsReceivingValidationException exception) { return BadRequest(ApiResponse<object>.Fail(400, exception.Message)); }
    }

    /// <summary>Pilihan supplier untuk saringan (hanya supplier yang memiliki Goods Receipt), dengan izin Logistik yang sama.</summary>
    [HttpGet("supplier-options")]
    [AccessAction("Read", "Read Procurement Inspection", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("LogisticsProcurementInspection", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProcurementInspectionSupplierOptionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupplierOptions(
        [FromQuery] ProcurementInspectionSupplierOptionsQuery query, CancellationToken cancellationToken)
    {
        var result = await _service.GetSupplierOptionsAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<ProcurementInspectionSupplierOptionResponse>>.Ok(result, "Pilihan supplier berhasil diambil."));
    }

    /// <summary>Detail satu Goods Receipt beserta seluruh barisnya, dengan identitas PO, supplier, dan status penghubung persediaan.</summary>
    [HttpGet("{goodsReceiptId:guid}")]
    [AccessAction("ReadDetail", "Read Procurement Inspection Detail", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("LogisticsProcurementInspection", "ReadDetail")]
    [ProducesResponseType(typeof(ApiResponse<ProcurementInspectionDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(Guid goodsReceiptId, CancellationToken cancellationToken)
    {
        var detail = await _service.GetDetailAsync(goodsReceiptId, cancellationToken);
        if (detail is null) return NotFound(ApiResponse<object>.Fail(404, "Dokumen penerimaan barang tidak ditemukan."));
        return Ok(ApiResponse<ProcurementInspectionDetailResponse>.Ok(detail, "Detail pemeriksaan hasil pengadaan berhasil diambil."));
    }
}
