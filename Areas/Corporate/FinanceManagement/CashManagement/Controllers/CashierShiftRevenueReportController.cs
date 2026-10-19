using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.Controllers;

/// <summary>
/// Layanan API Laporan Pendapatan Kasir per Shift (Read-Only Reporting).
/// Mendukung filter kasir, lokasi, shift, rincian transaksi, Petty Cash, asuransi, dan ekspor.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/reports/cashier-shifts")]
[Route("v1/corporate/finance-management/reports/cashier-shifts")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_CASHIER_SHIFT_REPORT", "Corporate Finance Management Cashier Shift Report", "FinanceCashierShiftReport",
    AreaName = "Corporate", ControllerName = "FinanceCashierShiftReport", Description = "Laporan Pendapatan Kasir per Shift", SortOrder = 56)]
[Tags("Corporate / Finance Management / Cashier Shift Report")]
public sealed class CashierShiftRevenueReportController : ControllerBase
{
    private readonly CashierShiftRevenueReportService _service;

    public CashierShiftRevenueReportController(CashierShiftRevenueReportService service)
    {
        _service = service;
    }

    [HttpGet("filters/registers")]
    [AccessAction("Read", "Read Register Options", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceCashierShiftReport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<FilterOptionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRegisterOptions(
        [FromQuery] RegisterFilterQuery query, CancellationToken cancellationToken)
    {
        var result = await _service.GetRegisterOptionsAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<FilterOptionResponse>>.Ok(result, "Daftar opsi register kasir berhasil diambil."));
    }

    [HttpGet("filters/cashiers")]
    [AccessAction("Read", "Read Cashier Options", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceCashierShiftReport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<FilterOptionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCashierOptions(
        [FromQuery] CashierFilterQuery query, CancellationToken cancellationToken)
    {
        var result = await _service.GetCashierOptionsAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<FilterOptionResponse>>.Ok(result, "Daftar opsi kasir berhasil diambil."));
    }

    [HttpGet("filters/locations")]
    [AccessAction("Read", "Read Location Options", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("FinanceCashierShiftReport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<FilterOptionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLocationOptions(
        [FromQuery] LocationFilterQuery query, CancellationToken cancellationToken)
    {
        var result = await _service.GetLocationOptionsAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<FilterOptionResponse>>.Ok(result, "Daftar opsi lokasi kasir berhasil diambil."));
    }

    [HttpGet("filters/shifts")]
    [AccessAction("Read", "Read Shift Options", AccessType = AccessTypes.Read, SortOrder = 3)]
    [AccessPermission("FinanceCashierShiftReport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<FilterOptionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShiftOptions(
        [FromQuery] ShiftFilterQuery query, CancellationToken cancellationToken)
    {
        var result = await _service.GetShiftOptionsAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<FilterOptionResponse>>.Ok(result, "Daftar opsi shift kasir berhasil diambil."));
    }

    [HttpGet]
    [AccessAction("Read", "Read Cashier Shift Report", AccessType = AccessTypes.Read, SortOrder = 4)]
    [AccessPermission("FinanceCashierShiftReport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<CashierShiftRevenueReportResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReport(
        [FromQuery] CashierShiftRevenueReportQuery query, CancellationToken cancellationToken)
    {
        if (query.DateFrom.HasValue && query.DateTo.HasValue && query.DateFrom.Value > query.DateTo.Value)
        {
            return BadRequest(ApiResponse<object>.Fail(
                StatusCodes.Status400BadRequest, "Tanggal awal (DateFrom) tidak boleh lebih besar dari tanggal akhir (DateTo)."));
        }

        var result = await _service.GetReportAsync(query, cancellationToken);
        return Ok(ApiResponse<CashierShiftRevenueReportResponse>.Ok(result, "Laporan pendapatan kasir per shift berhasil diambil."));
    }

    [HttpGet("{shiftId}/transactions")]
    [AccessAction("Read", "Read Shift Transactions", AccessType = AccessTypes.Read, SortOrder = 5)]
    [AccessPermission("FinanceCashierShiftReport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ShiftTransactionItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShiftTransactions(
        Guid shiftId, [FromQuery] ShiftTransactionQuery query, CancellationToken cancellationToken)
    {
        if (shiftId == Guid.Empty)
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "ShiftId tidak valid."));
        }

        try
        {
            var result = await _service.GetShiftTransactionsAsync(shiftId, query, cancellationToken);
            return Ok(ApiResponse<PagedResult<ShiftTransactionItemDto>>.Ok(result, "Rincian transaksi shift berhasil diambil."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
    }

    [HttpGet("{shiftId}/petty-cash")]
    [AccessAction("Read", "Read Shift Petty Cash", AccessType = AccessTypes.Read, SortOrder = 6)]
    [AccessPermission("FinanceCashierShiftReport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ShiftPettyCashItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShiftPettyCash(
        Guid shiftId, [FromQuery] ShiftPettyCashQuery query, CancellationToken cancellationToken)
    {
        if (shiftId == Guid.Empty)
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "ShiftId tidak valid."));
        }

        try
        {
            var result = await _service.GetShiftPettyCashAsync(shiftId, query, cancellationToken);
            return Ok(ApiResponse<PagedResult<ShiftPettyCashItemDto>>.Ok(result, "Transaksi Petty Cash shift berhasil diambil."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
    }

    [HttpGet("insurance")]
    [AccessAction("Read", "Read Insurance Report", AccessType = AccessTypes.Read, SortOrder = 7)]
    [AccessPermission("FinanceCashierShiftReport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InsuranceReportItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInsuranceReport(
        [FromQuery] InsuranceReportQuery query, CancellationToken cancellationToken)
    {
        if (query.DateFrom.HasValue && query.DateTo.HasValue && query.DateFrom.Value > query.DateTo.Value)
        {
            return BadRequest(ApiResponse<object>.Fail(
                StatusCodes.Status400BadRequest, "Tanggal awal (DateFrom) tidak boleh lebih besar dari tanggal akhir (DateTo)."));
        }

        var result = await _service.GetInsuranceReportAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<InsuranceReportItemDto>>.Ok(result, "Laporan tagihan dan pembayaran asuransi berhasil diambil."));
    }

    [HttpGet("export")]
    [AccessAction("Read", "Export Cashier Shift Report", AccessType = AccessTypes.Read, SortOrder = 8)]
    [AccessPermission("FinanceCashierShiftReport", "Read")]
    public async Task<IActionResult> Export(
        [FromQuery] CashierShiftExportQuery query, CancellationToken cancellationToken)
    {
        if (query.DateFrom.HasValue && query.DateTo.HasValue && query.DateFrom.Value > query.DateTo.Value)
        {
            return BadRequest(ApiResponse<object>.Fail(
                StatusCodes.Status400BadRequest, "Tanggal awal (DateFrom) tidak boleh lebih besar dari tanggal akhir (DateTo)."));
        }

        var (bytes, contentType, fileName) = await _service.ExportAsync(query, cancellationToken);
        return File(bytes, contentType, fileName);
    }
}
