using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using BloodBankProcedurePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs.BloodBankProcedureListDto>;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Controllers
{
    /// <summary>
    /// Tindakan Bank Darah — dicatat atas satu order beserta salinan tarifnya, lalu dinyatakan
    /// selesai. Penyelesaian menyerahkan satu fakta biaya ke Billing (<c>DEC-BD-016</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Empat endpoint kontrak <c>v4</c></b> — daftar, detail, catat, dan selesai — ditambah
    /// <c>resend-cost-fact</c> dari <c>BE-BD-013</c>. Tidak ada <c>GET /options</c>, <c>PUT</c>,
    /// <c>PATCH</c>, maupun <c>DELETE</c>: tindakan yang sudah terjadi tidak disunting atau dihapus.
    /// </para>
    /// <para>
    /// <b>Tidak ada isian Billing dari client.</b> Konteks sumber, jenis efek, kunjungan, identitas
    /// fakta, dan nominal seluruhnya diturunkan backend dari data tersimpan.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/blood-bank-management/blood-bank-procedures")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_BLOOD_BANK_MANAGEMENT",
        moduleName: "Health Service Blood Bank Management",
        displayName: "Blood Bank Procedure",
        AreaName = "HealthServices",
        ControllerName = "BloodBankProcedure",
        Description = "Mencatat dan menyelesaikan tindakan Bank Darah beserta salinan tarifnya",
        SortOrder = 1
    )]
    [Tags("Health Services / Blood Bank Management / Blood Bank Procedure")]
    public class BbkBloodBankProcedureController : ControllerBase
    {
        private const string LogCategory = "HealthServices.BloodBankManagement.BloodBankProcedure";
        private const string NotFoundMessage = "Tindakan Bank Darah tidak ditemukan atau sudah dihapus.";

        private readonly BbkBloodBankProcedureService _procedureService;
        private readonly LoggerService _loggerService;

        public BbkBloodBankProcedureController(
            BbkBloodBankProcedureService procedureService,
            LoggerService loggerService)
        {
            _procedureService = procedureService;
            _loggerService = loggerService;
        }

        /// <summary>Daftar tindakan Bank Darah, dengan penyaring dan halaman.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<BloodBankProcedurePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Bank Procedure", Description = "Melihat daftar tindakan Bank Darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodBankProcedure", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] Guid? bloodOrderId,
            [FromQuery] Guid? patientId,
            [FromQuery] BbkProcedureStatus? procedureStatus,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _procedureService.GetPagedAsync(
                search,
                bloodOrderId,
                patientId,
                procedureStatus,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<BloodBankProcedurePagedResult>.Ok(
                result,
                "Daftar tindakan Bank Darah berhasil diambil."));
        }

        /// <summary>Detail tindakan beserta konteks kunjungan, salinan tarif, dan riwayatnya.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BloodBankProcedureDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Blood Bank Procedure", Description = "Melihat detail tindakan Bank Darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodBankProcedure", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var detail = await _procedureService.GetDetailAsync(id, cancellationToken);

            if (detail == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, NotFoundMessage));

            return Ok(ApiResponse<BloodBankProcedureDetailDto>.Ok(
                detail,
                "Detail tindakan Bank Darah berhasil diambil."));
        }

        /// <summary>Mencatat tindakan atas satu order darah.</summary>
        /// <remarks>
        /// Unit dan kelas pasien diambil dari kunjungan order; tarif dan nominalnya dipilih backend
        /// dari data induk (<c>DEC-BD-048</c>, <c>DEC-BD-049</c>). Petugas pencatat diambil dari akun
        /// yang login.
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BloodBankProcedureDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Blood Bank Procedure", Description = "Mencatat tindakan Bank Darah", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("BloodBankProcedure", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateBloodBankProcedureRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _procedureService.CreateAsync(request, GetCurrentUserId(), cancellationToken);

            if (result.Outcome != BloodBankProcedureOutcome.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "BloodBankProcedure.Create",
                "Mencatat tindakan Bank Darah.",
                new
                {
                    EntityId = result.Entity!.Id,
                    result.Entity.ProcedureNumber,
                    result.Entity.BloodOrderId,
                    result.Entity.TariffId,
                    Controller = "BloodBankProcedure",
                    Action = "Create"
                });

            return Ok(ApiResponse<BloodBankProcedureDetailDto>.Ok(
                await BuildDetailAsync(result.Entity!.Id, cancellationToken),
                result.Message));
        }

        /// <summary>Menyatakan tindakan selesai.</summary>
        /// <remarks>
        /// Hanya tindakan berstatus <c>Recorded</c> yang dapat diselesaikan. Sesudah penyelesaian
        /// tersimpan, satu fakta biaya diserahkan ke Billing; hasilnya ada pada <c>BillingHandoff</c>.
        /// Kegagalan Billing tidak membatalkan penyelesaian — jawabannya tetap <c>200</c>, dan pesannya
        /// menyebut bila penyerahan memerlukan tinjauan.
        /// </remarks>
        [HttpPost("{id:guid}/complete")]
        [ProducesResponseType(typeof(ApiResponse<BloodBankProcedureDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Complete Blood Bank Procedure", Description = "Menyatakan tindakan Bank Darah selesai", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BloodBankProcedure", "Update")]
        public async Task<IActionResult> Complete(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _procedureService.CompleteAsync(id, GetCurrentUserId(), cancellationToken);

            if (result.Outcome != BloodBankProcedureOutcome.Success)
                return MapFailure(result);

            await LogHandoffAsync("BloodBankProcedure.Complete", "Menyatakan tindakan Bank Darah selesai.", result);

            var detail = await BuildDetailAsync(result.Entity!.Id, cancellationToken);
            var handoff = MapHandoff(result.BillingHandoff);

            if (detail != null)
                detail.BillingHandoff = handoff;

            var message = IsHandedOff(result.BillingHandoff)
                ? result.Message
                : "Tindakan Bank Darah dinyatakan selesai, tetapi penyerahan fakta biaya ke Billing memerlukan tinjauan.";

            return Ok(ApiResponse<BloodBankProcedureDetailDto>.Ok(detail, message));
        }

        /// <summary>Mengirim ulang fakta biaya tindakan yang sudah selesai.</summary>
        /// <remarks>
        /// <para>
        /// Bukan penyelesaian kedua: status, riwayat, dan salinan tarif tidak bergerak. Fakta disusun
        /// ulang dari data tersimpan sehingga identik dengan kiriman pertama, dan Billing tidak pernah
        /// membuat charge kedua (<c>AC-BD-027</c>). Dipakai untuk memulihkan penyerahan yang ditolak
        /// atau belum terkonfirmasi.
        /// </para>
        /// <para>
        /// Hak aksesnya sama dengan <c>complete</c> (<c>BloodBankProcedure : Update</c>), sehingga baris
        /// pada layar Akses Role tidak bertambah dan teks atributnya sengaja sama persis.
        /// </para>
        /// <para>
        /// <c>200</c> bila Billing menerima atau mengenali fakta sebagai kiriman ulang;
        /// <c>409</c> bila hasil kiriman sebelumnya belum pasti dan menuntut rekonsiliasi;
        /// <c>422</c> bila fakta ditolak Billing atau tidak dapat disusun, atau tindakan belum selesai.
        /// Ringkasan penyerahan ikut pada <c>errors</c> untuk jawaban gagal.
        /// </para>
        /// </remarks>
        [HttpPost("{id:guid}/resend-cost-fact")]
        [ProducesResponseType(typeof(ApiResponse<BloodBankProcedureDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Complete Blood Bank Procedure", Description = "Menyatakan tindakan Bank Darah selesai", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BloodBankProcedure", "Update")]
        public async Task<IActionResult> ResendCostFact(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _procedureService.ResendCostFactAsync(id, GetCurrentUserId(), cancellationToken);

            if (result.Outcome != BloodBankProcedureOutcome.Success)
                return MapFailure(result);

            await LogHandoffAsync("BloodBankProcedure.ResendCostFact", "Mengirim ulang fakta biaya tindakan Bank Darah.", result);

            var handoff = MapHandoff(result.BillingHandoff);

            switch (result.BillingHandoff?.Kind)
            {
                case ClinicalFactEmissionKind.Emitted:
                case ClinicalFactEmissionKind.Replayed:
                    var detail = await BuildDetailAsync(result.Entity!.Id, cancellationToken);

                    if (detail != null)
                        detail.BillingHandoff = handoff;

                    return Ok(ApiResponse<BloodBankProcedureDetailDto>.Ok(detail, result.Message));

                case ClinicalFactEmissionKind.OutcomeUnknown:
                case ClinicalFactEmissionKind.ReconciliationRequired:
                    return Conflict(ApiResponse<object>.Fail(
                        StatusCodes.Status409Conflict,
                        "Hasil penyerahan fakta biaya ke Billing belum dapat dipastikan. Rekonsiliasi diperlukan sebelum mengirim ulang.",
                        handoff));

                default:
                    return UnprocessableEntity(ApiResponse<object>.Fail(
                        StatusCodes.Status422UnprocessableEntity,
                        "Fakta biaya tindakan Bank Darah tidak diterima Billing.",
                        handoff));
            }
        }

        private async Task<BloodBankProcedureDetailDto?> BuildDetailAsync(Guid id, CancellationToken cancellationToken)
            => await _procedureService.GetDetailAsync(id, cancellationToken);

        private static bool IsHandedOff(ClinicalFactEmissionResult? handoff)
            => handoff?.Kind is ClinicalFactEmissionKind.Emitted or ClinicalFactEmissionKind.Replayed;

        private static BloodBankProcedureBillingHandoffDto? MapHandoff(ClinicalFactEmissionResult? handoff)
            => handoff == null
                ? null
                : new BloodBankProcedureBillingHandoffDto
                {
                    Kind = handoff.Kind.ToString(),
                    IsClinicallySafe = handoff.IsClinicallySafe,
                    MilestoneFactId = handoff.MilestoneFactId,
                    MilestoneFactVersion = handoff.MilestoneFactVersion,
                    DispatchStatus = handoff.DispatchStatus?.ToString(),
                    Code = handoff.Code,
                    Message = handoff.Message
                };

        /// <summary>Log berisi identitas dan kode saja — tanpa nama pasien maupun narasi klinis.</summary>
        private Task LogHandoffAsync(string action, string message, BloodBankProcedureResult result)
            => _loggerService.InfoAsync(
                LogCategory,
                action,
                message,
                new
                {
                    EntityId = result.Entity!.Id,
                    result.Entity.ProcedureNumber,
                    result.Entity.BloodOrderId,
                    BillingHandoff = result.BillingHandoff?.Kind.ToString(),
                    BillingHandoffCode = result.BillingHandoff?.Code,
                    result.BillingHandoff?.MilestoneFactId,
                    result.BillingHandoff?.MilestoneFactVersion,
                    Controller = "BloodBankProcedure",
                    Action = "Update"
                });

        /// <remarks>
        /// <c>Invalid</c> → <c>400</c> (termasuk <c>VAL-BD-026</c>); <c>TariffUnavailable</c> →
        /// <c>422 VAL-BD-084</c>; <c>NotAllowedByState</c> → <c>422</c>; <c>VersionConflict</c> →
        /// <c>409</c>; <c>NotFound</c> → <c>404</c>.
        /// </remarks>
        private IActionResult MapFailure(BloodBankProcedureResult result)
            => result.Outcome switch
            {
                BloodBankProcedureOutcome.NotFound => NotFound(
                    ApiResponse<object>.Fail(StatusCodes.Status404NotFound, result.Message)),

                BloodBankProcedureOutcome.VersionConflict => Conflict(
                    ApiResponse<object>.Fail(StatusCodes.Status409Conflict, result.Message)),

                BloodBankProcedureOutcome.TariffUnavailable or
                BloodBankProcedureOutcome.NotAllowedByState => UnprocessableEntity(
                    ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, result.Message)),

                _ => BadRequest(
                    ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, result.Message))
            };

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
