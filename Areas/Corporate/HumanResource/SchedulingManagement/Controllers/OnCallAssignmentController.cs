using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/corporate/human-resource/scheduling/on-call-assignments")]
    [Route("api/v1/corporate/human-resource/scheduling-management/on-call-assignments")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_SCHEDULING_MANAGEMENT",
        moduleName: "Human Resource Scheduling Management",
        displayName: "On-Call Assignment Administration",
        AreaName = "Corporate",
        ControllerName = "OnCallAssignment",
        Description = "Penjadwalan staf siaga on-call 24/7 dan aktivasi pemanggilan tugas darurat",
        SortOrder = 24)]
    [Tags("HR Scheduling - On-Call Management")]
    public class OnCallAssignmentController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.SchedulingManagement";
        private readonly OnCallAssignmentService _service;
        private readonly OnCallCompensationHandoffService _handoffService;
        private readonly OnCallCallService _callService;
        private readonly LoggerService _loggerService;

        public OnCallAssignmentController(
            OnCallAssignmentService service,
            OnCallCompensationHandoffService handoffService,
            OnCallCallService callService,
            LoggerService loggerService)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _handoffService = handoffService ?? throw new ArgumentNullException(nameof(handoffService));
            _callService = callService ?? throw new ArgumentNullException(nameof(callService));
            _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
        }

        // ===== Cadangan siaga dan catatan panggilan (BE-HRD-352; HRD-DEC-198, 204, 205) =====

        [HttpGet("{id:guid}/backups")]
        [AccessAction("Read", "Read On-Call Assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OnCallAssignment", "Read")]
        public async Task<IActionResult> GetBackups(Guid id, CancellationToken cancellationToken) =>
            ToActionResult(await _service.GetBackupsAsync(id, cancellationToken));

        [HttpPut("{id:guid}/backups")]
        [AccessAction("ManageBackup", "Manage On-Call Backups", Description = "Menetapkan daftar cadangan siaga berurutan", AccessType = AccessTypes.Update, SortOrder = 10)]
        [AccessPermission("OnCallAssignment", "ManageBackup")]
        public async Task<IActionResult> SetBackups(Guid id, [FromBody] SetOnCallBackupsRequest request, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            var result = await _service.SetBackupsAsync(id, request, userId, cancellationToken);
            if (result.Success)
            {
                await AuditAsync("OnCallAssignment.SetBackups", $"Menetapkan {result.Data!.Count} cadangan siaga penugasan {id}: " +
                    string.Join(", ", result.Data.Select(x => $"{x.BackupSequence}={x.DisplayName ?? x.WorkforceProfileId.ToString()}")) + ".");
            }

            return ToActionResult(result);
        }

        [HttpGet("{id:guid}/calls")]
        [AccessAction("Read", "Read On-Call Assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OnCallAssignment", "Read")]
        public async Task<IActionResult> GetCalls(Guid id, CancellationToken cancellationToken) =>
            ToActionResult(await _callService.GetCallsAsync(id, cancellationToken));

        [HttpPost("{id:guid}/calls")]
        [AccessAction("RecordCall", "Record On-Call Call", Description = "Mencatat panggilan siaga dan jawabannya", AccessType = AccessTypes.Update, SortOrder = 11)]
        [AccessPermission("OnCallAssignment", "RecordCall")]
        public async Task<IActionResult> CreateCall(Guid id, [FromBody] CreateOnCallCallRequest request, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return await CallResultAsync("Create", await _callService.CreateCallAsync(id, request, userId, cancellationToken));
        }

        [HttpPost("{id:guid}/calls/{callId:guid}/accept")]
        [AccessAction("RecordCall", "Record On-Call Call", Description = "Mencatat panggilan siaga dan jawabannya", AccessType = AccessTypes.Update, SortOrder = 11)]
        [AccessPermission("OnCallAssignment", "RecordCall")]
        public async Task<IActionResult> AcceptCall(Guid id, Guid callId, [FromBody] AnswerOnCallCallRequest request, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return await CallResultAsync("Accept", await _callService.AcceptAsync(id, callId, request, userId, cancellationToken));
        }

        [HttpPost("{id:guid}/calls/{callId:guid}/decline")]
        [AccessAction("RecordCall", "Record On-Call Call", Description = "Mencatat panggilan siaga dan jawabannya", AccessType = AccessTypes.Update, SortOrder = 11)]
        [AccessPermission("OnCallAssignment", "RecordCall")]
        public async Task<IActionResult> DeclineCall(Guid id, Guid callId, [FromBody] DeclineOnCallCallRequest request, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return await CallResultAsync("Decline", await _callService.DeclineAsync(id, callId, request, userId, cancellationToken));
        }

        [HttpPost("{id:guid}/calls/{callId:guid}/expire")]
        [AccessAction("RecordCall", "Record On-Call Call", Description = "Mencatat panggilan siaga dan jawabannya", AccessType = AccessTypes.Update, SortOrder = 11)]
        [AccessPermission("OnCallAssignment", "RecordCall")]
        public async Task<IActionResult> ExpireCall(Guid id, Guid callId, [FromBody] ExpireOnCallCallRequest request, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return await CallResultAsync("Expire", await _callService.ExpireAsync(id, callId, request, userId, DateTime.UtcNow, cancellationToken));
        }

        [HttpPost("{id:guid}/calls/{callId:guid}/complete")]
        [AccessAction("RecordCall", "Record On-Call Call", Description = "Mencatat panggilan siaga dan jawabannya", AccessType = AccessTypes.Update, SortOrder = 11)]
        [AccessPermission("OnCallAssignment", "RecordCall")]
        public async Task<IActionResult> CompleteCall(Guid id, Guid callId, [FromBody] CompleteOnCallCallRequest request, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return await CallResultAsync("Complete", await _callService.CompleteAsync(id, callId, request, userId, DateTime.UtcNow, cancellationToken));
        }

        // Log audit tanpa alasan panggilan/penolakan (kolom sensitif kamus data); isi di teks pesan karena payload tidak disimpan.
        private async Task<IActionResult> CallResultAsync(string action, SchedulingRequestServiceResult<OnCallCallResponse> result)
        {
            if (result.Success)
            {
                var c = result.Data!;
                await AuditAsync($"OnCallCall.{action}",
                    $"Panggilan siaga {c.Id} penugasan {c.OnCallAssignmentId}: {c.CallStatus}; dipanggil {c.CalledDisplayName ?? c.CalledWorkforceProfileId.ToString()} (urutan {c.BackupSequence}); unit pemanggil {c.CallingOrganizationUnitName ?? c.CallingOrganizationUnitId.ToString()}" +
                    (c.ResponseMinutes.HasValue ? $"; respons {c.ResponseMinutes} menit" : string.Empty) + ".");
            }

            return ToActionResult(result);
        }

        private Task AuditAsync(string action, string message) => _loggerService.AuditAsync(LogCategory, action, message);

        [HttpGet]
        [AccessAction("Read", "Read On-Call Assignments", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OnCallAssignment", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] Guid? rosterPeriodId = null,
            [FromQuery] Guid? workforceProfileId = null,
            [FromQuery] string? status = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPagedAsync(
                pageNumber,
                pageSize,
                rosterPeriodId,
                workforceProfileId,
                status,
                cancellationToken);

            return Ok(ApiResponse<PagedResult<OnCallAssignmentResponse>>.Ok(
                result,
                "Daftar penugasan siaga (on-call) berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read On-Call Assignment Detail", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("OnCallAssignment", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return ToActionResult(await _service.GetByIdAsync(id, cancellationToken));
        }

        [HttpPost]
        [AccessAction("Create", "Create On-Call Assignment", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("OnCallAssignment", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateOnCallAssignmentRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.CreateAsync(request, userId, cancellationToken));
        }

        [HttpPost("{id:guid}/confirm")]
        [AccessAction("Update", "Confirm On-Call Assignment", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("OnCallAssignment", "Update")]
        public async Task<IActionResult> Confirm(
            Guid id,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.ConfirmAsync(id, userId, cancellationToken));
        }

        [HttpPost("{id:guid}/activate")]
        [AccessAction("Update", "Activate On-Call Assignment", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("OnCallAssignment", "Update")]
        public async Task<IActionResult> Activate(
            Guid id,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.ActivateAsync(id, userId, cancellationToken));
        }

        [HttpPost("{id:guid}/complete")]
        [AccessAction("Update", "Complete On-Call Assignment", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("OnCallAssignment", "Update")]
        public async Task<IActionResult> Complete(
            Guid id,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.CompleteAsync(id, userId, cancellationToken));
        }

        /// <summary>
        /// Endpoint perekaman pemanggilan cito dokter/staf siaga dan evaluasi SLA respon kedatangan (BE-HRD-100, BE-HRD-102, HRD-DEC-103).
        /// Dapat diakses oleh unit klinis pemanggil (Laboratorium, IGD, OK) maupun Kepala Unit.
        /// </summary>
        [HttpPost("{id:guid}/call-in")]
        [AccessAction("Update", "Record On-Call Call-In", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("OnCallAssignment", "Update")]
        public async Task<IActionResult> CallIn(
            Guid id,
            [FromBody] RecordOnCallCallInRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.RecordCallInAsync(id, request, userId, cancellationToken));
        }

        /// <summary>
        /// Endpoint rekapitulasi data kompensasi siaga standby dan tiket call-in untuk modul Payroll (10) dan Lembur (09) (BE-HRD-101, BE-HRD-102, INT-HR-38).
        /// </summary>
        [HttpGet("handoff/summary")]
        [AccessAction("Read", "Get On-Call Compensation Summary", AccessType = AccessTypes.Read, SortOrder = 8)]
        [AccessPermission("OnCallAssignment", "Read")]
        public async Task<IActionResult> GetCompensationSummary(
            [FromQuery] Guid? rosterPeriodId = null,
            [FromQuery] Guid? hospitalSiteId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            CancellationToken cancellationToken = default)
        {
            return ToActionResult(await _handoffService.GetOnCallCompensationSummaryAsync(
                rosterPeriodId,
                hospitalSiteId,
                startDate,
                endDate,
                cancellationToken));
        }

        /// <summary>
        /// Endpoint pembacaan dokter siaga yang berstatus 'Active' untuk integrasi Lab (BE-HRD-099, LAB-DEC-111, INT-HR-37).
        /// </summary>
        [HttpGet("active-doctors")]
        [AccessAction("Read", "Get Active On-Call Doctors", AccessType = AccessTypes.Read, SortOrder = 9)]
        [AccessPermission("OnCallAssignment", "Read")]
        public async Task<IActionResult> GetActiveDoctors(
            [FromQuery] Guid? hospitalSiteId = null,
            [FromQuery] DateTime? effectiveAt = null,
            CancellationToken cancellationToken = default)
        {
            var targetTime = effectiveAt ?? DateTime.UtcNow;
            var doctorIds = await _service.GetActiveOnCallDoctorIdsAsync(
                hospitalSiteId,
                targetTime,
                cancellationToken);

            return Ok(ApiResponse<List<Guid>>.Ok(
                doctorIds,
                $"Ditemukan {doctorIds.Count} dokter siaga berstatus Active pada waktu {targetTime:yyyy-MM-dd HH:mm:ss} UTC."));
        }

        private IActionResult ToActionResult<T>(SchedulingRequestServiceResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        private bool TryGetUserId(out Guid userId)
        {
            var value = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out userId) && userId != Guid.Empty;
        }

        private IActionResult UnauthorizedResponse()
        {
            return Unauthorized(ApiResponse<object>.Fail(
                StatusCodes.Status401Unauthorized,
                "Identitas user login tidak valid."));
        }
    }
}
