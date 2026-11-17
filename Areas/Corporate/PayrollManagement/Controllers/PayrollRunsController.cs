using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Controllers;

/// <summary>
/// Siklus payroll run milik modul Payroll: buka, impor data HRD, hitung, ajukan, kembalikan, sahkan, batalkan, tutup. Potongan cicilan
/// piutang RS dihitung saat run dihitung dan hasilnya dikonfirmasi ke Finance hanya saat run disahkan. Tidak ada eksekusi potongan
/// manual per pegawai. Arketipe transaksi: aggregate ber-lifecycle (standar transaksi bagian 2.1).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/payroll-management/payroll-runs")]
// Alias kompatibilitas: rute sebelum Payroll dipisah dari HRD. Hapus bertahap setelah seluruh consumer berpindah ke rute utama.
[Route("api/v1/corporate/human-resource/payroll-runs")]
// ModuleCode/ControllerName sengaja TIDAK diubah: layar Akses Role menautkan hak admin ke kunci ini.
[AccessController(
    moduleCode: "CORPORATE_HUMAN_RESOURCE_PAYROLL_RUN",
    moduleName: "Corporate Human Resource Payroll Run",
    displayName: "Payroll Run",
    AreaName = "Corporate",
    ControllerName = "PayrollRun",
    Description = "Siklus payroll run: buka, hitung, ajukan, sahkan, batalkan",
    SortOrder = 55)]
[Tags("Corporate / Payroll Management / Payroll Run")]
public sealed class PayrollRunsController : ControllerBase
{
    private readonly PayrollRunService _service;
    private readonly PayrollHrdImportService _hrdImportService;
    private readonly PayrollSimulationService _simulationService;

    public PayrollRunsController(
        PayrollRunService service,
        PayrollHrdImportService hrdImportService,
        PayrollSimulationService simulationService)
    {
        _service = service;
        _hrdImportService = hrdImportService;
        _simulationService = simulationService;
    }

    private Guid CurrentUserId() => PayrollControllerSupport.CurrentUserId(User);

    private Task<IActionResult> Run<T>(Func<Task<T>> action, string message) => PayrollControllerSupport.RunAsync(action, message);

    // ------------------------------------------------------------------------------------
    // Baca
    // ------------------------------------------------------------------------------------

    [HttpGet]
    [AccessAction("Read", "Read Payroll Run", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PayrollRun", "Read")]
    public async Task<IActionResult> GetPaged([FromQuery] PayrollRunQuery query, CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<PayrollRunListItemResponse>>.Ok(result, "Daftar payroll run berhasil dimuat."));
    }

    [HttpGet("summary")]
    [AccessAction("ReadSummary", "Read Payroll Run Summary", AccessType = AccessTypes.Read, SortOrder = 14)]
    [AccessPermission("PayrollRun", "ReadSummary")]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        Run(() => _service.GetSummaryAsync(cancellationToken), "Ringkasan payroll run berhasil dimuat.");

    [HttpGet("filters/metadata")]
    [AccessAction("ReadMetadata", "Read Payroll Run Filter Metadata", AccessType = AccessTypes.Read, SortOrder = 15)]
    [AccessPermission("PayrollRun", "ReadMetadata")]
    public IActionResult GetFilterMetadata() =>
        Ok(ApiResponse<PayrollRunFilterMetadataResponse>.Ok(PayrollRunService.GetFilterMetadata(), "Metadata filter payroll run berhasil dimuat."));

    [HttpGet("period-options")]
    [AccessAction("ReadPeriods", "Read Payroll Run Periods", AccessType = AccessTypes.Read, SortOrder = 9)]
    [AccessPermission("PayrollRun", "ReadPeriods")]
    public async Task<IActionResult> GetPeriodOptions(CancellationToken cancellationToken) =>
        Ok(ApiResponse<List<PayrollRunPeriodOptionResponse>>.Ok(await _service.GetPeriodOptionsAsync(cancellationToken), "Periode payroll berhasil dimuat."));

    /// <summary>
    /// PAY-BE-21: hitung percobaan satu periode tanpa menyimpan apa pun (SIM-01..03). Tidak mengimpor HR, tidak menyentuh Finance,
    /// dan tidak menerbitkan nomor; lihat Notes pada respons.
    /// </summary>
    [HttpPost("simulations")]
    [AccessAction("Simulate", "Simulate Payroll Run", Description = "Menghitung percobaan payroll satu periode tanpa menyimpan data", AccessType = AccessTypes.Read, SortOrder = 21)]
    [AccessPermission("PayrollRun", "Simulate")]
    public Task<IActionResult> Simulate([FromBody] PayrollSimulationRequest request, CancellationToken cancellationToken) =>
        Run(() => _simulationService.SimulateAsync(request, CurrentUserId(), cancellationToken), "Simulasi payroll selesai. Tidak ada data yang disimpan.");

    [HttpGet("period-overview")]
    [AccessAction("ReadPeriodOverview", "Read Payroll Run Period Overview", AccessType = AccessTypes.Read, SortOrder = 10)]
    [AccessPermission("PayrollRun", "ReadPeriodOverview")]
    public Task<IActionResult> GetPeriodOverview([FromQuery] Guid payrollPeriodId, CancellationToken cancellationToken) =>
        Run(() => _service.GetPeriodOverviewAsync(payrollPeriodId, cancellationToken), "Ringkasan periode payroll berhasil dimuat.");

    [HttpGet("{id:guid}")]
    [AccessAction("ReadDetail", "Read Payroll Run Detail", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("PayrollRun", "ReadDetail")]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        Run(() => _service.GetByIdAsync(id, cancellationToken), "Detail payroll run berhasil dimuat.");

    /// <summary>PAY-BE-23: rekap lembur terverifikasi HR yang diimpor ke run, per pegawai per tanggal (baca-saja, PRL-DEC-42).</summary>
    [HttpGet("{id:guid}/overtime-inputs")]
    [AccessAction("Read", "Read Payroll Run", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PayrollRun", "Read")]
    public Task<IActionResult> GetOvertimeInputs(Guid id, [FromQuery] PayrollRunOvertimeInputQuery query, CancellationToken cancellationToken) =>
        Run(() => _service.GetOvertimeInputsAsync(id, query, cancellationToken), "Rekap lembur payroll run berhasil dimuat.");

    [HttpGet("{id:guid}/status-history")]
    [AccessAction("ReadStatusHistory", "Read Payroll Run Status History", AccessType = AccessTypes.Read, SortOrder = 16)]
    [AccessPermission("PayrollRun", "ReadStatusHistory")]
    public Task<IActionResult> GetStatusHistory(Guid id, CancellationToken cancellationToken) =>
        Run(() => _service.GetStatusHistoryAsync(id, cancellationToken), "Riwayat status payroll run berhasil dimuat.");

    [HttpGet("{id:guid}/employees/{runEmployeeId:guid}")]
    [AccessAction("ReadEmployeeDetail", "Read Payroll Run Employee Detail", AccessType = AccessTypes.Read, SortOrder = 17)]
    [AccessPermission("PayrollRun", "ReadEmployeeDetail")]
    public Task<IActionResult> GetEmployeeDetail(Guid id, Guid runEmployeeId, CancellationToken cancellationToken) =>
        Run(() => _service.GetEmployeeDetailAsync(id, runEmployeeId, cancellationToken), "Rincian penerima berhasil dimuat.");

    [HttpGet("{id:guid}/hrd-readiness")]
    [AccessAction("ReadHrdReadiness", "Read Payroll Run HRD Readiness", AccessType = AccessTypes.Read, SortOrder = 18)]
    [AccessPermission("PayrollRun", "ReadHrdReadiness")]
    public Task<IActionResult> GetHrdReadiness(Guid id, [FromQuery] PayrollHrdReadinessQuery query, CancellationToken cancellationToken) =>
        Run(() => _hrdImportService.GetReadinessAsync(id, query, cancellationToken), "Kesiapan data HRD berhasil dimuat.");

    // ------------------------------------------------------------------------------------
    // Aksi (perpindahan status)
    // ------------------------------------------------------------------------------------

    [HttpPost]
    [AccessAction("Create", "Open Payroll Run", AccessType = AccessTypes.Create, SortOrder = 3)]
    [AccessPermission("PayrollRun", "Create")]
    public Task<IActionResult> Create([FromBody] CreatePayrollRunRequest request, CancellationToken cancellationToken) =>
        Run(() => _service.CreateAsync(request, CurrentUserId(), cancellationToken), "Payroll run berhasil dibuka.");

    [HttpPost("{id:guid}/import-hrd")]
    [AccessAction("ImportHrd", "Import HRD Data Into Payroll Run", AccessType = AccessTypes.Update, SortOrder = 19)]
    [AccessPermission("PayrollRun", "ImportHrd")]
    public Task<IActionResult> ImportHrd(
        Guid id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] PayrollHrdImportRequest? request, CancellationToken cancellationToken) =>
        Run(() => _hrdImportService.ImportAsync(id, request ?? new PayrollHrdImportRequest(), CurrentUserId(), cancellationToken),
            "Impor data HRD selesai diproses.");

    [HttpPost("{id:guid}/calculate")]
    [AccessAction("Calculate", "Calculate Payroll Run", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("PayrollRun", "Calculate")]
    public Task<IActionResult> Calculate(
        Guid id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] PayrollRunActionRequest? request, CancellationToken cancellationToken) =>
        Run(() => _service.CalculateAsync(id, request?.ExpectedRowVersion, CurrentUserId(), cancellationToken), "Payroll run berhasil dihitung.");

    [HttpPost("{id:guid}/submit")]
    [AccessAction("Submit", "Submit Payroll Run", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("PayrollRun", "Submit")]
    public Task<IActionResult> Submit(
        Guid id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] PayrollRunActionRequest? request, CancellationToken cancellationToken) =>
        Run(() => _service.SubmitAsync(id, request?.ExpectedRowVersion, CurrentUserId(), cancellationToken), "Payroll run berhasil diajukan.");

    [HttpPost("{id:guid}/return")]
    [AccessAction("Return", "Return Payroll Run", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("PayrollRun", "Return")]
    public Task<IActionResult> Return(Guid id, [FromBody] PayrollRunDecisionRequest request, CancellationToken cancellationToken) =>
        Run(() => _service.ReturnAsync(id, request?.Reason, request?.ExpectedRowVersion, CurrentUserId(), cancellationToken),
            "Payroll run dikembalikan ke tahap hitung.");

    [HttpPost("{id:guid}/approve")]
    [AccessAction("Approve", "Approve Payroll Run", AccessType = AccessTypes.Update, SortOrder = 7)]
    [AccessPermission("PayrollRun", "Approve")]
    public Task<IActionResult> Approve(
        Guid id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] PayrollRunActionRequest? request, CancellationToken cancellationToken) =>
        Run(() => _service.ApproveAsync(id, request?.ExpectedRowVersion, CurrentUserId(), cancellationToken),
            "Payroll run disahkan dan hasil potongan dikonfirmasi ke Finance.");

    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Cancel", "Cancel Payroll Run", AccessType = AccessTypes.Update, SortOrder = 8)]
    [AccessPermission("PayrollRun", "Cancel")]
    public Task<IActionResult> Cancel(Guid id, [FromBody] PayrollRunDecisionRequest request, CancellationToken cancellationToken) =>
        Run(() => _service.CancelAsync(id, request?.Reason, request?.ExpectedRowVersion, CurrentUserId(), cancellationToken), "Payroll run dibatalkan.");

    [HttpPost("{id:guid}/close")]
    [AccessAction("Close", "Close Payroll Run", AccessType = AccessTypes.Update, SortOrder = 20)]
    [AccessPermission("PayrollRun", "Close")]
    public Task<IActionResult> Close(
        Guid id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] PayrollRunActionRequest? request, CancellationToken cancellationToken) =>
        Run(() => _service.CloseAsync(id, request?.ExpectedRowVersion, CurrentUserId(), cancellationToken), "Payroll run ditutup.");

    // ------------------------------------------------------------------------------------
    // Pembalikan potongan cicilan atas run yang sudah disahkan
    // ------------------------------------------------------------------------------------

    [HttpPost("{id:guid}/employees/{runEmployeeId:guid}/reversals")]
    [AccessAction("RequestReversal", "Request Payroll Deduction Reversal", AccessType = AccessTypes.Create, SortOrder = 11)]
    [AccessPermission("PayrollRun", "RequestReversal")]
    public Task<IActionResult> RequestReversal(Guid id, Guid runEmployeeId, [FromBody] RequestPayrollReversalRequest request, CancellationToken cancellationToken) =>
        Run(() => _service.RequestReversalAsync(id, runEmployeeId, request, CurrentUserId(), cancellationToken), "Pembalikan potongan diajukan.");

    [HttpPost("{id:guid}/reversals/{reversalId:guid}/approve")]
    [AccessAction("ApproveReversal", "Approve Payroll Deduction Reversal", AccessType = AccessTypes.Update, SortOrder = 12)]
    [AccessPermission("PayrollRun", "ApproveReversal")]
    public Task<IActionResult> ApproveReversal(Guid id, Guid reversalId, CancellationToken cancellationToken) =>
        Run(() => _service.ApproveReversalAsync(id, reversalId, CurrentUserId(), cancellationToken), "Pembalikan disetujui dan saldo piutang dipulihkan di Finance.");

    [HttpPost("{id:guid}/reversals/{reversalId:guid}/reject")]
    [AccessAction("RejectReversal", "Reject Payroll Deduction Reversal", AccessType = AccessTypes.Update, SortOrder = 13)]
    [AccessPermission("PayrollRun", "RejectReversal")]
    public Task<IActionResult> RejectReversal(Guid id, Guid reversalId, [FromBody] PayrollRunDecisionRequest request, CancellationToken cancellationToken) =>
        Run(() => _service.RejectReversalAsync(id, reversalId, request?.Reason, CurrentUserId(), cancellationToken), "Pembalikan ditolak.");
}
