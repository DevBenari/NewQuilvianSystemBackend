using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers
{
    /// <summary>
    /// Layar penghunian tempat tidur: mencari tempat tidur, memesan, menempatkan pasien, dan
    /// memindahkannya.
    /// </summary>
    /// <remarks>
    /// <b>Penolakan kelayakan mengembalikan daftar aturan yang gagal.</b> Jawaban 422 pada
    /// controller ini memuat kolom <c>errors</c> berisi daftar aturan Kelayakan Penempatan
    /// yang tidak terpenuhi, bukan satu kalimat umum. Petugas perlu tahu apakah yang
    /// menghalangi adalah keadaan tempat tidurnya, jenis kelamin pasien, atau kebutuhan
    /// isolasinya, karena tindakan lanjutannya berbeda untuk masing-masing.
    ///
    /// <para>
    /// <b>Yang belum ada di sini.</b> Pelepasan tempat tidur saat pasien pergi
    /// (<c>BE-RWI-027</c>) dan saat episode ditutup (<c>BE-RWI-025</c>) bukan endpoint pada
    /// controller ini; keduanya milik grup Inpatient Discharge.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/inpatient-management/bed-occupancies")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_INPATIENT",
        moduleName: "Health Service Inpatient",
        displayName: "Inpatient Bed Occupancy",
        AreaName = "HealthServices",
        ControllerName = "InpatientBedOccupancy",
        Description = "Mencari, memesan, menempatkan, dan memindahkan tempat tidur rawat inap",
        SortOrder = 11
    )]
    [Tags("Health Services / Inpatient Management / Bed Occupancy")]
    public class InpatientBedOccupancyController : ControllerBase
    {
        private const string LogCategory = "HealthServices.InPatientManagement.BedOccupancy";

        private readonly InpBedOccupancyService _bedOccupancyService;
        private readonly LoggerService _loggerService;

        public InpatientBedOccupancyController(
            InpBedOccupancyService bedOccupancyService,
            LoggerService loggerService)
        {
            _bedOccupancyService = bedOccupancyService;
            _loggerService = loggerService;
        }

        // =====================================================================
        // BE-RWI-010 — Pencarian dan papan ketersediaan
        // =====================================================================

        /// <summary>
        /// Mencari tempat tidur yang benar-benar dapat ditempati.
        /// </summary>
        /// <remarks>
        /// Kirim <c>episodeId</c> bila pencarian dilakukan untuk satu pasien tertentu. Dengan
        /// <c>episodeId</c>, hasilnya disaring memakai seluruh aturan Kelayakan Penempatan —
        /// jenis kelamin pasien dan kebutuhan isolasinya ikut diperhitungkan, sehingga tempat
        /// tidur yang muncul di sini tidak akan ditolak saat petugas menekan simpan.
        /// </remarks>
        [HttpGet("available-beds")]
        [ProducesResponseType(typeof(ApiResponse<AvailableBedPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Inpatient Bed Occupancy", Description = "Mencari tempat tidur rawat inap yang tersedia", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientBedOccupancy", "Read")]
        public async Task<IActionResult> GetAvailableBeds(
            [FromQuery] AvailableBedQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _bedOccupancyService.SearchAvailableBedsAsync(query, cancellationToken);

            return Ok(ApiResponse<AvailableBedPagedResult>.Ok(
                result,
                "Daftar tempat tidur yang tersedia berhasil diambil."));
        }

        /// <summary>Papan ketersediaan tempat tidur per unit layanan dan kamar.</summary>
        [HttpGet("bed-board")]
        [ProducesResponseType(typeof(ApiResponse<BedBoardResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Inpatient Bed Occupancy", Description = "Melihat papan ketersediaan tempat tidur", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientBedOccupancy", "Read")]
        public async Task<IActionResult> GetBedBoard(
            [FromQuery] Guid? serviceUnitId,
            CancellationToken cancellationToken = default)
        {
            var result = await _bedOccupancyService.GetBedBoardAsync(serviceUnitId, cancellationToken);

            return Ok(ApiResponse<BedBoardResponse>.Ok(
                result,
                "Papan ketersediaan tempat tidur berhasil diambil."));
        }

        /// <summary>Memesan tempat tidur untuk satu episode <c>Draft</c>.</summary>
        /// <remarks>
        /// Pemesanan mengunci tempat tidur selama batas waktu yang ditetapkan admin pada
        /// master pengaturan, lalu gugur sendiri tanpa program penjadwal. Angka batasnya
        /// dibaca ulang setiap pemesanan, sehingga perubahan admin berlaku pada pemesanan
        /// berikutnya.
        /// </remarks>
        [HttpPost("reservations")]
        [ProducesResponseType(typeof(ApiResponse<BedReservationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Inpatient Bed Occupancy", Description = "Memesan dan menempatkan pasien ke tempat tidur rawat inap", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("InpatientBedOccupancy", "Create")]
        public async Task<IActionResult> ReserveBed(
            [FromBody] ReserveBedRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bedOccupancyService.ReserveBedAsync(
                request,
                User.GetUserId(),
                cancellationToken);

            if (result.Status != InpEpisodeOperationStatus.Success)
            {
                return FromFailure(result);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientBedOccupancy.ReserveBed",
                "Memesan tempat tidur rawat inap.",
                new
                {
                    EntityId = result.ReservationId,
                    Controller = "InpatientBedOccupancy",
                    Action = "ReserveBed",
                    StatusCode = StatusCodes.Status200OK
                });

            var reservation = await _bedOccupancyService.GetReservationAsync(
                result.ReservationId!.Value,
                cancellationToken);

            return Ok(ApiResponse<BedReservationResponse>.Ok(reservation, result.Message));
        }

        /// <summary>Membatalkan pemesanan sebelum dipakai.</summary>
        [HttpPatch("reservations/{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<BedReservationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Cancel Inpatient Bed Reservation", Description = "Membatalkan pemesanan tempat tidur", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InpatientBedOccupancy", "Update")]
        public async Task<IActionResult> CancelReservation(
            Guid id,
            [FromBody] CancelReservationRequest? request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bedOccupancyService.CancelReservationAsync(
                id,
                request,
                User.GetUserId(),
                cancellationToken);

            if (result.Status != InpEpisodeOperationStatus.Success)
            {
                return FromFailure(result);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientBedOccupancy.CancelReservation",
                "Membatalkan pemesanan tempat tidur.",
                new
                {
                    EntityId = id,
                    Controller = "InpatientBedOccupancy",
                    Action = "CancelReservation",
                    StatusCode = StatusCodes.Status200OK
                });

            var reservation = await _bedOccupancyService.GetReservationAsync(id, cancellationToken);

            return Ok(ApiResponse<BedReservationResponse>.Ok(reservation, result.Message));
        }

        // =====================================================================
        // BE-RWI-011 — Penempatan pasien
        // =====================================================================

        /// <summary>Menempatkan pasien ke tempat tidur dan mengaktifkan episodenya.</summary>
        /// <remarks>
        /// Bila permintaan ditolak, isian admisi tetap utuh dan episode tetap <c>Draft</c>.
        /// Petugas cukup memilih tempat tidur lain tanpa mengisi ulang apa pun.
        /// </remarks>
        [HttpPost("placements")]
        [ProducesResponseType(typeof(ApiResponse<BedPlacementResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Inpatient Bed Occupancy", Description = "Memesan dan menempatkan pasien ke tempat tidur rawat inap", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("InpatientBedOccupancy", "Create")]
        public async Task<IActionResult> PlacePatient(
            [FromBody] PlacePatientRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bedOccupancyService.PlacePatientAsync(
                request,
                User.GetUserId(),
                cancellationToken);

            if (result.Status != InpEpisodeOperationStatus.Success)
            {
                return FromFailure(result);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientBedOccupancy.PlacePatient",
                "Menempatkan pasien ke tempat tidur rawat inap.",
                new
                {
                    EntityId = result.PlacementId,
                    Controller = "InpatientBedOccupancy",
                    Action = "PlacePatient",
                    StatusCode = StatusCodes.Status200OK
                });

            var placement = await _bedOccupancyService.GetPlacementAsync(
                result.PlacementId!.Value,
                cancellationToken);

            return Ok(ApiResponse<BedPlacementResponse>.Ok(placement, result.Message));
        }

        // =====================================================================
        // BE-RWI-019 — Perpindahan pasien
        // =====================================================================

        /// <summary>Memindahkan pasien ke tempat tidur lain dalam satu tindakan utuh.</summary>
        /// <remarks>
        /// Bila pembukaan penempatan baru gagal, penempatan lama tidak jadi ditutup dan pasien
        /// tetap berada di tempat semula. Tidak pernah ada satu saat pun pasien tercatat tanpa
        /// tempat tidur.
        /// </remarks>
        [HttpPost("placements/transfer")]
        [ProducesResponseType(typeof(ApiResponse<BedPlacementResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Transfer", "Transfer Inpatient Bed Placement", Description = "Memindahkan pasien ke tempat tidur lain", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("InpatientBedOccupancy", "Transfer")]
        public async Task<IActionResult> TransferPatient(
            [FromBody] TransferPatientRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bedOccupancyService.TransferAsync(
                request,
                User.GetUserId(),
                User.GetDoctorId(),
                cancellationToken);

            if (result.Status != InpEpisodeOperationStatus.Success)
            {
                return FromFailure(result);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientBedOccupancy.TransferPatient",
                "Memindahkan pasien ke tempat tidur lain.",
                new
                {
                    EntityId = result.PlacementId,
                    Controller = "InpatientBedOccupancy",
                    Action = "TransferPatient",
                    StatusCode = StatusCodes.Status200OK
                });

            var placement = await _bedOccupancyService.GetPlacementAsync(
                result.PlacementId!.Value,
                cancellationToken);

            return Ok(ApiResponse<BedPlacementResponse>.Ok(placement, result.Message));
        }

        /// <summary>Riwayat penempatan satu episode, dari tempat tidur pertama sampai terakhir.</summary>
        [HttpGet("placements/by-episode/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<BedPlacementResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Inpatient Bed Occupancy", Description = "Melihat riwayat penempatan satu episode", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientBedOccupancy", "Read")]
        public async Task<IActionResult> GetPlacementsByEpisode(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var result = await _bedOccupancyService.GetPlacementsByEpisodeAsync(
                episodeId,
                cancellationToken);

            return Ok(ApiResponse<List<BedPlacementResponse>>.Ok(
                result,
                "Riwayat penempatan berhasil diambil."));
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        /// <summary>
        /// Menerjemahkan penolakan service menjadi kode status, sekaligus menyertakan daftar
        /// aturan Kelayakan Penempatan yang gagal pada kolom <c>errors</c>.
        /// </summary>
        private IActionResult FromFailure(InpBedOccupancyOperationResult result)
        {
            object? errors = result.Failures.Count > 0 ? result.Failures : null;

            return result.Status switch
            {
                InpEpisodeOperationStatus.Invalid => BadRequest(
                    ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        result.Message,
                        errors)),

                InpEpisodeOperationStatus.Forbidden => StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status403Forbidden,
                        result.Message,
                        errors)),

                InpEpisodeOperationStatus.NotFound => NotFound(
                    ApiResponse<object>.Fail(
                        StatusCodes.Status404NotFound,
                        result.Message,
                        errors)),

                InpEpisodeOperationStatus.Conflict => Conflict(
                    ApiResponse<object>.Fail(
                        StatusCodes.Status409Conflict,
                        result.Message,
                        errors)),

                _ => StatusCode(
                    StatusCodes.Status422UnprocessableEntity,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status422UnprocessableEntity,
                        result.Message,
                        errors))
            };
        }
    }
}
