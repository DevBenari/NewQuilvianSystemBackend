using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers;

[ApiController, Authorize]
[ServiceFilter(typeof(InpBedOperationFilter))]
[Route("api/v1/health-services/inpatient-management/bed-management")]
[AccessController(moduleCode: "HEALTH_SERVICE_INPATIENT", moduleName: "Health Service Inpatient",
    displayName: "Inpatient Bed Management", AreaName = "HealthServices", ControllerName = "InpatientBedManagement",
    Description = "Monitoring, pembersihan, verifikasi, dan riwayat penggunaan bed", SortOrder = 12)]
[Tags("Health Services / Inpatient Management / Bed Management")]
public class InpatientBedManagementController(InpBedManagementQueryService queries,
    InpBedReadinessService readiness, InpBedAccessService access, InpFloorPlanQueryService floorPlans) : ControllerBase
{
    /// <summary>
    /// Denah satu lantai untuk Monitoring Bed: status kamar, badge, ringkasan HD/LD, dan bed dengan
    /// enam status (BE-RWI-238). Tanpa <c>floorCode</c>, lantai dengan bed Tersedia terbanyak.
    /// Profil OperationalOnly tidak menerima nama, No. RM, maupun HD/LD.
    /// </summary>
    [HttpGet("floor-plan")]
    [ProducesResponseType(typeof(ApiResponse<FloorPlanResponse>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Inpatient Bed Management", AccessType = AccessTypes.Read)]
    [AccessPermission("InpatientBedManagement", "Read")]
    public async Task<IActionResult> FloorPlan([FromQuery] FloorPlanMonitoringQuery query, CancellationToken ct = default)
    {
        var result = await floorPlans.GetMonitoringAsync(query, ct);
        return Ok(ApiResponse<FloorPlanResponse>.Ok(result, "Denah lantai berhasil diambil."));
    }

    [HttpGet("metadata")]
    [AccessAction("Read", "Read Inpatient Bed Management", AccessType = AccessTypes.Read)]
    [AccessPermission("InpatientBedManagement", "Read")]
    public async Task<IActionResult> Metadata([FromQuery] Guid? serviceUnitId, CancellationToken ct)
    {
        var actions = new List<string>();
        foreach (var action in new[] { "Read", "ReadUsageHistory", "StartCleaning", "CompleteCleaning", "VerifyReadiness" })
        {
            var units = await access.UnitsAsync("InpatientBedManagement:" + action, ct);
            if (units.Count > 0 && (!serviceUnitId.HasValue || units.Contains(serviceUnitId.Value)))
            {
                if (action is "StartCleaning" or "CompleteCleaning" or "VerifyReadiness")
                {
                    var allowed = false;
                    foreach (var unit in units.Where(x => !serviceUnitId.HasValue || x == serviceUnitId.Value))
                        try { access.WorkflowAssignment(unit, "InpatientBedManagement:" + action); allowed = true; } catch (InpBedOperationException) { }
                    if (!allowed) continue;
                }
                actions.Add(action);
            }
        }
        return Ok(ApiResponse<object>.Ok(new {
            Tabs = new[] { new { key = "monitoring", label = "Monitoring", allowed = actions.Contains("Read") },
                new { key = "housekeeping", label = "Pembersihan", allowed = actions.Contains("StartCleaning") || actions.Contains("CompleteCleaning") || actions.Contains("VerifyReadiness") },
                new { key = "transfer", label = "Transfer", allowed = actions.Contains("Read") },
                new { key = "history", label = "Riwayat penggunaan", allowed = actions.Contains("ReadUsageHistory") } },
            BedStatuses = InpBedManagementQueryService.StatusLabels.Select(x => new { code = x.Key, label = x.Value }),
            TransferCategories = new[] { new { code = 1, label = "DownGrade" }, new { code = 2, label = "UpGrade" }, new { code = 3, label = "SameGrade" } },
            AllowedActions = actions }, "Metadata bed tersedia."));
    }

    [HttpGet("monitoring")]
    [AccessAction("Read", "Read Inpatient Bed Management", AccessType = AccessTypes.Read)]
    [AccessPermission("InpatientBedManagement", "Read")]
    public async Task<IActionResult> Monitoring([FromQuery] BedMonitoringQuery query, CancellationToken ct = default)
    {
        var result = await queries.MonitoringAsync(query, ct: ct);
        return Ok(ApiResponse<object>.Ok(result, "Data bed berhasil diproses."));
    }

    [HttpGet("usage-history")]
    [AccessAction("ReadUsageHistory", "ReadUsageHistory Inpatient Bed Management", AccessType = AccessTypes.Read)]
    [AccessPermission("InpatientBedManagement", "ReadUsageHistory")]
    public async Task<IActionResult> History([FromQuery] BedUsageHistoryQuery query, CancellationToken ct = default)
    {
        var result = await queries.HistoryAsync(query, ct);
        return Ok(ApiResponse<object>.Ok(result, "Data bed berhasil diproses."));
    }

    [HttpGet("operations/{key}")]
    [AccessAction("Read", "Read Inpatient Bed Management", AccessType = AccessTypes.Read)]
    [AccessPermission("InpatientBedManagement", "Read")]
    public async Task<IActionResult> Outcome(string key, CancellationToken ct = default)
    {
        var result = await queries.OutcomeAsync(key, ct);
        return Ok(ApiResponse<object>.Ok(result, "Data bed berhasil diproses."));
    }

    [HttpGet("beds/{bedId:guid}/cleaning-attempts")]
    [AccessAction("Read", "Read Inpatient Bed Management", AccessType = AccessTypes.Read)]
    [AccessPermission("InpatientBedManagement", "Read")]
    public async Task<IActionResult> Attempts(Guid bedId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
    {
        var result = await readiness.AttemptsAsync(bedId, pageNumber, pageSize, ct);
        return Ok(ApiResponse<object>.Ok(result, "Data bed berhasil diproses."));
    }

    [HttpPost("beds/{bedId:guid}/cleaning-attempts")]
    [AccessAction("StartCleaning", "StartCleaning Inpatient Bed Management", AccessType = AccessTypes.Create)]
    [AccessPermission("InpatientBedManagement", "StartCleaning")]
    public async Task<IActionResult> Start(Guid bedId, [FromBody] StartBedCleaningRequest request, CancellationToken ct = default)
    {
        var result = await readiness.StartAsync(bedId, request, ct);
        return Ok(ApiResponse<object>.Ok(result, "Data bed berhasil diproses."));
    }

    [HttpPatch("cleaning-attempts/{attemptId:guid}/complete")]
    [AccessAction("CompleteCleaning", "CompleteCleaning Inpatient Bed Management", AccessType = AccessTypes.Update)]
    [AccessPermission("InpatientBedManagement", "CompleteCleaning")]
    public async Task<IActionResult> Complete(Guid attemptId, [FromBody] CompleteBedCleaningRequest request, CancellationToken ct = default)
    {
        var result = await readiness.CompleteAsync(attemptId, request, ct);
        return Ok(ApiResponse<object>.Ok(result, "Data bed berhasil diproses."));
    }

    [HttpPost("beds/{bedId:guid}/readiness-verifications")]
    [AccessAction("VerifyReadiness", "VerifyReadiness Inpatient Bed Management", AccessType = AccessTypes.Update)]
    [AccessPermission("InpatientBedManagement", "VerifyReadiness")]
    public async Task<IActionResult> Verify(Guid bedId, [FromBody] VerifyBedReadinessRequest request, CancellationToken ct = default)
    {
        var result = await readiness.VerifyAsync(bedId, request, ct);
        return Ok(ApiResponse<object>.Ok(result, "Data bed berhasil diproses."));
    }
}

