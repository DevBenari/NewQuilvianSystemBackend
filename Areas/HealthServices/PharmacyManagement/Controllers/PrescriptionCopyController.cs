using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers;

/// <summary>
/// Copy resep: keterangan berapa obat yang sudah diserahkan dan berapa sisanya.
/// </summary>
/// <remarks>
/// <para>
/// Angkanya berasal dari histori penyerahan yang benar-benar mengurangi stok, bukan dari
/// status resep. Menerbitkan copy resep tidak mengubah stok, catatan penyerahan, maupun status
/// resep.
/// </para>
/// <para>
/// Identitas fasilitas dan nomor izin praktik apoteker dibaca dari master dan
/// <b>tidak pernah diminta sebagai isian</b>. Bila kredensialnya belum terisi, kekosongan itu
/// dilaporkan lewat <c>missingDocumentData</c> — tidak diisi nilai karangan.
/// </para>
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/health-services/pharmacy-management/prescription-copies")]
[Tags("Health Services / Pharmacy Management / Prescription Copy")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_PHARMACY_MANAGEMENT",
    moduleName: "Health Service Pharmacy Management",
    displayName: "Prescription Copy",
    AreaName = "HealthServices",
    ControllerName = "PrescriptionCopy",
    Description = "Copy resep",
    SortOrder = 8)]
public class PrescriptionCopyController : ControllerBase
{
    private readonly PrescriptionCopyService _service;

    public PrescriptionCopyController(PrescriptionCopyService service) => _service = service;

    /// <summary>
    /// Pratinjau copy resep tanpa menerbitkan apa pun.
    /// </summary>
    /// <remarks>
    /// Angkanya dihitung langsung dari histori penyerahan, sehingga dapat berubah bila
    /// penyerahan berlanjut. Yang sudah diterbitkan tidak ikut berubah.
    /// </remarks>
    [HttpGet("preview/{prescriptionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PrescriptionCopyPreviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Prescription Copy",
        Description = "Melihat pratinjau dan dokumen copy resep",
        AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PrescriptionCopy", "Read")]
    public async Task<IActionResult> GetPreview(Guid prescriptionId,
        [FromQuery] Guid? pharmacistWorkforceId, CancellationToken cancellationToken)
    {
        var data = await _service.GetPreviewAsync(prescriptionId, pharmacistWorkforceId,
            cancellationToken);
        return data == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound,
                "Resep tidak ditemukan."))
            : Ok(ApiResponse<PrescriptionCopyPreviewResponse>.Ok(data,
                "Pratinjau copy resep berhasil diambil."));
    }

    /// <summary>Lembar copy resep yang pernah terbit untuk sebuah resep.</summary>
    [HttpGet("by-prescription/{prescriptionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<PrescriptionCopySummaryResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Prescription Copy",
        Description = "Melihat daftar copy resep sebuah resep",
        AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("PrescriptionCopy", "Read")]
    public async Task<IActionResult> GetByPrescription(Guid prescriptionId,
        CancellationToken cancellationToken)
    {
        var data = await _service.GetByPrescriptionAsync(prescriptionId, cancellationToken);
        return Ok(ApiResponse<List<PrescriptionCopySummaryResponse>>.Ok(data,
            "Daftar copy resep berhasil diambil."));
    }

    /// <summary>Satu lembar copy resep yang sudah diterbitkan, beserta angka yang tercetak padanya.</summary>
    [HttpGet("{prescriptionCopyId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PrescriptionCopyDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Prescription Copy",
        Description = "Melihat detail copy resep", AccessType = AccessTypes.Read, SortOrder = 3)]
    [AccessPermission("PrescriptionCopy", "Read")]
    public async Task<IActionResult> GetById(Guid prescriptionCopyId,
        CancellationToken cancellationToken)
    {
        var data = await _service.GetDetailAsync(prescriptionCopyId, cancellationToken);
        return data == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound,
                "Copy resep tidak ditemukan."))
            : Ok(ApiResponse<PrescriptionCopyDetailResponse>.Ok(data,
                "Copy resep berhasil diambil."));
    }

    /// <summary>
    /// Menerbitkan satu lembar copy resep.
    /// </summary>
    /// <remarks>
    /// Angkanya dibekukan saat terbit, karena lembarnya berpindah tangan dan apotek lain
    /// bertindak atas angka yang tertera padanya.
    /// </remarks>
    [HttpPost("issue/{prescriptionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PrescriptionCopyDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Issue", "Issue Prescription Copy",
        Description = "Menerbitkan copy resep", AccessType = AccessTypes.Create, SortOrder = 4)]
    [AccessPermission("PrescriptionCopy", "Issue")]
    public Task<IActionResult> Issue(Guid prescriptionId,
        [FromBody] IssuePrescriptionCopyRequest request, CancellationToken cancellationToken) =>
        RunAsync(() => _service.IssueAsync(prescriptionId, request, cancellationToken),
            "Copy resep berhasil diterbitkan.");

    /// <summary>
    /// Mencabut sebuah lembar yang sudah terbit.
    /// </summary>
    /// <remarks>
    /// Lembarnya tidak dihapus; ia mungkin sudah berpindah tangan, dan riwayat harus tetap
    /// menjelaskan bahwa dokumen itu pernah ada beserta alasan pencabutannya.
    /// </remarks>
    [HttpPatch("{prescriptionCopyId:guid}/revoke")]
    [ProducesResponseType(typeof(ApiResponse<PrescriptionCopyDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Revoke", "Revoke Prescription Copy",
        Description = "Mencabut copy resep", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("PrescriptionCopy", "Revoke")]
    public Task<IActionResult> Revoke(Guid prescriptionCopyId,
        [FromBody] RevokePrescriptionCopyRequest request, CancellationToken cancellationToken) =>
        RunAsync(() => _service.RevokeAsync(prescriptionCopyId, request, cancellationToken),
            "Copy resep berhasil dicabut.");

    /// <summary>Menerjemahkan kegagalan domain menjadi status HTTP yang sesuai.</summary>
    private async Task<IActionResult> RunAsync(
        Func<Task<PrescriptionCopyDetailResponse>> action, string message)
    {
        try
        {
            var data = await action();
            return Ok(ApiResponse<PrescriptionCopyDetailResponse>.Ok(data, message));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound,
                exception.Message));
        }
        catch (PrescriptionCopyConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict,
                exception.Message, new { code = exception.Code }));
        }
        catch (PrescriptionCopyUnprocessableException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(
                StatusCodes.Status422UnprocessableEntity, exception.Message,
                new { code = exception.Code }));
        }
        catch (PrescriptionCopyForbiddenException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, exception.Message));
        }
    }
}
