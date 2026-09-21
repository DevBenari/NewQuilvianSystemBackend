using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Controllers
{
    /// <summary>
    /// Laporan Patologi Anatomi dan konteks klinis, keduanya melekat pada satu <b>pesanan</b>
    /// (<c>LAB-DEC-085</c>, <c>r25</c> bagian 20.2).
    ///
    /// <b>Base URL-nya <c>lab-orders</c>, bukan <c>lab-examinations</c>, dan itu bukan
    /// kerapian.</b> Patolog menulis satu narasi diagnostik untuk seluruh bahan yang datang
    /// bersama; jalur per pemeriksaan pada <c>r24</c> salah alamat, dan itu sebab utama ia
    /// digantikan.
    ///
    /// <b><c>finalize</c> dan <c>reopen</c> dipisahkan dari <c>PUT</c>, juga disengaja.</b>
    /// Keduanya <b>pernyataan profesional</b>, bukan penyimpanan. Menggabungkannya ke dalam
    /// <c>PUT</c> membuat seseorang dapat memfinalkan laporan <b>tanpa sadar</b> hanya karena
    /// mengirim satu ruas tambahan.
    ///
    /// <b>Dua hak akses berbeda hidup pada satu controller.</b> Laporan memakai
    /// <c>LabExamination</c>; konteks klinis memakai <c>LabOrder</c>, sebab penulisnya
    /// <b>dokter pemesan</b>, bukan patolog (<c>LAB-DEC-091</c>, <c>INV-40</c>).
    ///
    /// <b>Yang TIDAK ada di sini:</b> memvalidasi, merilis, dan mengirim hasil. Ketiganya
    /// <c>S4e</c>, tertahan <c>DEC-LAB-011</c>. Nol status hasil diperkenalkan (<c>INV-36</c>).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-orders")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Pathology Report",
        AreaName = "HealthServices",
        ControllerName = "LabPathologyReport",
        Description = "Pengisian laporan Patologi Anatomi dan konteks klinis per pesanan",
        SortOrder = 15
    )]
    [Tags("Health Services / Laboratory Management / Lab Pathology Report")]
    public class LabPathologyReportController : ControllerBase
    {
        private readonly LabPathologyReportService _labPathologyReportService;

        public LabPathologyReportController(LabPathologyReportService labPathologyReportService)
        {
            _labPathologyReportService = labPathologyReportService;
        }

        // Membaca laporan beserta BENTUK FORMULIRNYA: ruas yang berlaku bagi pesanan ini, penanda
        // wajibnya, dan isinya bila sudah diisi. Layar nol perlu menebak bentuk formulirnya.
        //
        // Satu ruas yang dipakai dua golongan muncul SEKALI (AC-136). Pesanan yang belum
        // digolongkan menjawab daftar KOSONG beserta sebabnya (VAL-100) — bukan daftar kosong
        // tanpa penjelasan, sebab itu akan terbaca sebagai kerusakan.
        [HttpGet("{labOrderId:guid}/pathology-report")]
        [ProducesResponseType(typeof(ApiResponse<LabPathologyReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Read", "Read Lab Pathology Report", Description = "Melihat laporan Patologi Anatomi beserta ruas yang berlaku", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabExamination", "Read")]
        public Task<IActionResult> GetReport(
            Guid labOrderId,
            CancellationToken cancellationToken = default) =>
            ExecuteReportAsync(
                () => _labPathologyReportService.GetReportAsync(labOrderId, cancellationToken),
                "Laporan Patologi Anatomi berhasil diambil.");

        // Menyimpan SELURUH isi laporan sekaligus. Boleh sebagian terisi; kelengkapannya baru
        // diuji saat finalize (VAL-95).
        //
        // Ditolak 409 ketika laporan sudah final (VAL-96) — yang tersedia hanya reopen.
        // Ruas issuedAt, effectiveAt, waktu finalisasi, pelaku, dan nama parameter DITOLAK bila
        // dikirim (AC-140): seluruhnya diturunkan server, dan menerimanya berarti mengizinkan
        // laporan mengaku terbit pada waktu yang tidak pernah terjadi.
        [HttpPut("{labOrderId:guid}/pathology-report")]
        [ProducesResponseType(typeof(ApiResponse<LabPathologyReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Write Lab Pathology Report", Description = "Menyimpan isi laporan Patologi Anatomi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabExamination", "Update")]
        public Task<IActionResult> SaveReport(
            Guid labOrderId,
            [FromBody] LabPathologyReportRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteReportAsync(
                () => _labPathologyReportService.SaveReportAsync(labOrderId, request, cancellationToken),
                "Laporan Patologi Anatomi berhasil disimpan.");

        // Menyatakan laporan SELESAI DITULIS — bukan merilisnya (LAB-DEC-088, INV-35).
        //
        // Ditolak 422 beserta DAFTAR RUAS YANG KOSONG selama masih ada isian wajib yang belum
        // terisi (VAL-95). Menolak dengan "laporan belum lengkap" saja akan membuat patolog
        // menebak ruas mana yang terlewat pada formulir berisi sampai lima belas isian.
        [HttpPost("{labOrderId:guid}/pathology-report/finalize")]
        [ProducesResponseType(typeof(ApiResponse<LabPathologyReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Finalize Lab Pathology Report", Description = "Menyatakan laporan Patologi Anatomi selesai ditulis", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabExamination", "Update")]
        public Task<IActionResult> Finalize(
            Guid labOrderId,
            CancellationToken cancellationToken = default) =>
            ExecuteReportAsync(
                () => _labPathologyReportService.FinalizeAsync(labOrderId, cancellationToken),
                "Laporan Patologi Anatomi berhasil diselesaikan.");

        // Membuka kembali laporan yang sudah final. Alasannya WAJIB (VAL-97) dan meninggalkan
        // jejak audit tersendiri: sesudah difinalkan, patolog sudah menyatakan diagnosisnya
        // selesai, sehingga mengubahnya kembali perlu dapat dijelaskan.
        [HttpPost("{labOrderId:guid}/pathology-report/reopen")]
        [ProducesResponseType(typeof(ApiResponse<LabPathologyReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Reopen Lab Pathology Report", Description = "Membuka kembali laporan Patologi Anatomi yang sudah selesai", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabExamination", "Update")]
        public Task<IActionResult> Reopen(
            Guid labOrderId,
            [FromBody] LabPathologyReopenRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteReportAsync(
                () => _labPathologyReportService.ReopenAsync(labOrderId, request, cancellationToken),
                "Laporan Patologi Anatomi berhasil dibuka kembali.");

        // Membaca konteks klinis pesanan. Hak aksesnya LabOrder, bukan LabExamination.
        [HttpGet("{labOrderId:guid}/pathology-context")]
        [ProducesResponseType(typeof(ApiResponse<LabPathologyOrderContextResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Read", "Read Lab Pathology Context", Description = "Melihat konteks klinis pesanan Patologi Anatomi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabOrder", "Read")]
        public Task<IActionResult> GetContext(
            Guid labOrderId,
            CancellationToken cancellationToken = default) =>
            ExecuteContextAsync(
                () => _labPathologyReportService.GetContextAsync(labOrderId, cancellationToken),
                "Konteks klinis berhasil diambil.");

        // Menulis konteks klinis. PENULISNYA DOKTER PEMESAN, bukan patolog — karena itu hak
        // aksesnya LabOrder : Update, bukan LabExamination : Update (AC-141, LAB-DEC-091,
        // INV-40). Berbagi satu izin dengan patolog berarti memberi patolog hak menulis riwayat
        // penyakit pasien yang tidak pernah ia tanyakan, dan itu masuk rekam medis.
        [HttpPut("{labOrderId:guid}/pathology-context")]
        [ProducesResponseType(typeof(ApiResponse<LabPathologyOrderContextResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Write Lab Pathology Context", Description = "Menulis konteks klinis pesanan Patologi Anatomi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabOrder", "Update")]
        public Task<IActionResult> SaveContext(
            Guid labOrderId,
            [FromBody] LabPathologyOrderContextRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteContextAsync(
                () => _labPathologyReportService.SaveContextAsync(labOrderId, request, cancellationToken),
                "Konteks klinis berhasil disimpan.");

        /// <summary>
        /// Menerjemahkan kegagalan menjadi status HTTP yang tepat, tanpa membocorkan detail
        /// exception ke pemanggil.
        /// </summary>
        private async Task<IActionResult> ExecuteReportAsync(
            Func<Task<LabPathologyReportResponse>> action,
            string successMessage)
        {
            try
            {
                var result = await action();

                return Ok(ApiResponse<LabPathologyReportResponse>.Ok(result, successMessage));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
            catch (LabPathologyReportConflictException exception)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict, exception.Message));
            }
            catch (LabPathologyReportValidationException exception)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity, exception.Message));
            }
        }

        private async Task<IActionResult> ExecuteContextAsync(
            Func<Task<LabPathologyOrderContextResponse>> action,
            string successMessage)
        {
            try
            {
                var result = await action();

                return Ok(ApiResponse<LabPathologyOrderContextResponse>.Ok(result, successMessage));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
            catch (LabPathologyReportValidationException exception)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity, exception.Message));
            }
        }
    }
}
