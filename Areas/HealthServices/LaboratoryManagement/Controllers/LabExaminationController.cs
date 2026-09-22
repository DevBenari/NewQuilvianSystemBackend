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
    /// Pemeriksaan terpesan — satuan yang ditagihkan, terpisah dari wadah fisik yang
    /// menopangnya.
    ///
    /// Satu wadah menopang satu atau lebih pemeriksaan. Membatalkan satu pemeriksaan di sini
    /// tidak menyentuh pemeriksaan lain pada wadah yang sama; menggugurkan seluruh isi wadah
    /// adalah akibat penolakan wadah, dan itu pekerjaan grup Lab Specimen.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-examinations")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Examination",
        AreaName = "HealthServices",
        ControllerName = "LabExamination",
        Description = "Pengelolaan pemeriksaan terpesan laboratorium",
        SortOrder = 6
    )]
    [Tags("Health Services / Laboratory Management / Lab Examination")]
    public class LabExaminationController : ControllerBase
    {
        private readonly LabExaminationService _labExaminationService;
        private readonly LabMicrobiologyResultService _labMicrobiologyResultService;
        private readonly LabConfirmingDoctorResolver _confirmingDoctorResolver;

        public LabExaminationController(
            LabExaminationService labExaminationService,
            LabMicrobiologyResultService labMicrobiologyResultService,
            LabConfirmingDoctorResolver confirmingDoctorResolver)
        {
            _labExaminationService = labExaminationService;
            _labMicrobiologyResultService = labMicrobiologyResultService;
            _confirmingDoctorResolver = confirmingDoctorResolver;
        }

        // Daftar pemeriksaan terpesan pada satu pesanan.
        [HttpGet("by-order/{labOrderId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<LabExaminationResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Examination", Description = "Melihat pemeriksaan terpesan pada satu pesanan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabExamination", "Read")]
        public Task<IActionResult> GetByOrder(
            Guid labOrderId,
            CancellationToken cancellationToken = default) =>
            ExecuteListAsync(
                () => _labExaminationService.GetByOrderAsync(labOrderId, cancellationToken),
                "Daftar pemeriksaan terpesan berhasil diambil.");

        // Daftar pemeriksaan yang ditopang satu wadah. Inilah yang membuktikan satu tabung
        // dapat menopang beberapa pemeriksaan sekaligus (AC-35).
        [HttpGet("by-specimen/{specimenId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<LabExaminationResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Examination", Description = "Melihat pemeriksaan yang ditopang satu wadah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabExamination", "Read")]
        public Task<IActionResult> GetBySpecimen(
            Guid specimenId,
            CancellationToken cancellationToken = default) =>
            ExecuteListAsync(
                () => _labExaminationService.GetBySpecimenAsync(specimenId, cancellationToken),
                "Daftar pemeriksaan pada wadah berhasil diambil.");

        // Menambah pemeriksaan terpesan dan menautkannya ke wadah penopangnya.
        //
        // Harga tidak diterima dari pemanggil; backend menyalinnya dari tarif yang berlaku.
        // Jenis pemeriksaan yang bukan laboratorium ditolak 422 (VAL-17); tarif yang belum
        // diatur ditolak 422 (VAL-20); wadah yang sudah diputuskan ditolak 409 (VAL-18).
        [HttpPost("by-order/{labOrderId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabExaminationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Lab Examination", Description = "Menambah pemeriksaan terpesan", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LabExamination", "Create")]
        public Task<IActionResult> Add(
            Guid labOrderId,
            [FromBody] AddLabExaminationRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labExaminationService.AddAsync(labOrderId, request, cancellationToken),
                "Pemeriksaan terpesan berhasil ditambahkan.");

        // Membatalkan satu pemeriksaan terpesan. Pemeriksaan lain pada wadah yang sama tidak
        // berubah, dan status wadahnya sendiri tidak disentuh.
        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<LabExaminationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Cancel Lab Examination", Description = "Membatalkan satu pemeriksaan terpesan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabExamination", "Update")]
        public Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelLabExaminationRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labExaminationService.CancelAsync(id, request, cancellationToken),
                "Pemeriksaan terpesan berhasil dibatalkan.");

        // Menandai satu pemeriksaan cito atau mengembalikannya menjadi biasa.
        //
        // Kesegeraan melekat pada pemeriksaan, bukan pada pesanan (LAB-DEC-026). Tidak ada
        // endpoint sejenis pada grup Lab Order, dan ketiadaan itu disengaja (AC-40).
        //
        // Bukan dokter pemesan ditolak 403 (VAL-03); pesanan yang sudah selesai atau
        // dibatalkan ditolak 409 (VAL-04).
        [HttpPut("{id:guid}/urgency")]
        [ProducesResponseType(typeof(ApiResponse<LabExaminationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Set Lab Examination Urgency", Description = "Menandai satu pemeriksaan cito atau mengembalikannya biasa", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LabExamination", "Update")]
        public Task<IActionResult> SetUrgency(
            Guid id,
            [FromBody] SetLabExaminationUrgencyRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labExaminationService.SetUrgencyAsync(id, request, cancellationToken),
                "Kesegeraan pemeriksaan berhasil diperbarui.");

        // Menandai satu pemeriksaan dikerjakan ganda, atau membatalkan penandaannya.
        //
        // Penandaan hanya berlaku pada pemeriksaan itu sendiri; pemeriksaan lain pada wadah
        // yang sama tidak ikut berubah (AC-40). Wadah yang sudah ditolak ditolak 409.
        [HttpPut("{id:guid}/duplo")]
        [ProducesResponseType(typeof(ApiResponse<LabExaminationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Set Lab Examination Duplo", Description = "Menandai satu pemeriksaan dikerjakan ganda", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("LabExamination", "Update")]
        public Task<IActionResult> SetDuplo(
            Guid id,
            [FromBody] SetLabExaminationDuploRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labExaminationService.SetDuploAsync(id, request, cancellationToken),
                "Penanda duplo pemeriksaan berhasil diperbarui.");

        private async Task<IActionResult> ExecuteListAsync(
            Func<Task<List<LabExaminationResponse>>> action,
            string successMessage)
        {
            try
            {
                var result = await action();

                return Ok(ApiResponse<List<LabExaminationResponse>>.Ok(result, successMessage));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }

        [HttpGet("{id:guid}/result")]
        [ProducesResponseType(typeof(ApiResponse<LabExaminationResultFormResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Examination Result Form", Description = "Melihat bentuk hasil dan hasil yang sudah terisi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabExamination", "Read")]
        public async Task<IActionResult> GetResultForm(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            // r22. Layar tidak dapat menurunkan bentuk hasil sendiri: ia ditentukan batas nilai
            // yang berlaku bagi PASIEN tertentu, bukan oleh jenis pemeriksaannya saja.
            try
            {
                var result = await _labExaminationService.GetResultFormAsync(id, cancellationToken);

                return Ok(ApiResponse<LabExaminationResultFormResponse>.Ok(
                    result, "Bentuk hasil pemeriksaan berhasil diambil."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }

        [HttpPut("{id:guid}/result")]
        [ProducesResponseType(typeof(ApiResponse<LabExaminationResultResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Set Lab Examination Result", Description = "Mengisi hasil pemeriksaan laboratorium", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabExamination", "Update")]
        public async Task<IActionResult> SetResult(
            Guid id,
            [FromBody] LabExaminationResultRequest request,
            CancellationToken cancellationToken = default)
        {
            // Slice S4a. Endpoint ini MENGISI hasil — ia tidak memvalidasi, tidak merilis, tidak
            // menandai nilai kritis, dan tidak mengoreksi. Keempatnya tertahan LAB-SIGN-001
            // lewat LAB-DEC-003, LAB-DEC-004, dan LAB-DEC-007, dan nol status hasil disentuh
            // di sini.
            //
            // Hak akses memakai ulang LabExamination:Update, bukan resource baru: memecah izin
            // pengisian dari izin pemeriksaan lain berarti menetapkan pembagian wewenang yang
            // justru menunggu jawaban LAB-SIGN-001.
            try
            {
                var result = await _labExaminationService.SetResultAsync(id, request, cancellationToken);

                return Ok(ApiResponse<LabExaminationResultResponse>.Ok(
                    result, "Hasil pemeriksaan berhasil disimpan."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
            catch (LabExaminationConflictException exception)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict, exception.Message));
            }
            catch (LabExaminationValidationException exception)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity, exception.Message));
            }
        }

        /// <summary>
        /// Menjalankan satu perubahan dan menerjemahkan kegagalannya menjadi status HTTP yang
        /// tepat, tanpa membocorkan detail exception ke pemanggil.
        /// </summary>
        private async Task<IActionResult> ExecuteAsync(
            Func<Task<LabExaminationResponse>> action,
            string successMessage)
        {
            try
            {
                var result = await action();

                return Ok(ApiResponse<LabExaminationResponse>.Ok(result, successMessage));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
            catch (LabExaminationForbiddenException exception)
            {
                // VAL-03. Objek yang diminta memang ada dan pemanggilnya memang sudah masuk;
                // yang tidak dia miliki adalah kewenangan atas pesanan ini. Karena itu 403,
                // bukan 404 maupun 422.
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, exception.Message));
            }
            catch (LabExaminationConflictException exception)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict, exception.Message));
            }
            catch (LabExaminationValidationException exception)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity, exception.Message));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest, exception.Message));
            }
        }

        // =============================================================
        // Kelengkapan dan konsultasi hasil Mikrobiologi — BE-LAB-54, slice S4b
        //
        // KETIGA ENDPOINT DI BAWAH TIDAK MERILIS. Simpan Final berarti penulisnya selesai
        // menulis — sebuah fakta (LAB-DEC-097). Rilis Mikrobiologi adalah S4d, tertahan
        // DEC-LAB-011, dan nol endpoint rilis ada di sini.
        //
        // Hak akses memakai ulang LabExamination:Update sesuai LAB-PERM-v1 rev 8 bagian 10.1 —
        // nol resource permission baru.
        // =============================================================

        [HttpPost("{id:guid}/result/microbiology/finalize")]
        [ProducesResponseType(typeof(ApiResponse<LabExaminationCompletionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Finalize Lab Microbiology Result", Description = "Menyatakan penulisan hasil Mikrobiologi selesai", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LabExamination", "Update")]
        public async Task<IActionResult> FinalizeMicrobiologyResult(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            // Nol badan permintaan diterima, dan itu disengaja: endpoint ini menyatakan
            // "saya selesai", sedangkan seluruh isinya sudah tersimpan lewat PUT /result.
            // Menerima badan permintaan akan membuat dua jalur menulis hasil yang sama.
            try
            {
                var result = await _labExaminationService.FinalizeMicrobiologyResultAsync(id, cancellationToken);

                return Ok(ApiResponse<LabExaminationCompletionResponse>.Ok(
                    result, "Penulisan hasil dinyatakan selesai. Hasil ini belum dirilis."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
            catch (LabExaminationConflictException exception)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict, exception.Message));
            }
            catch (LabExaminationValidationException exception)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity, exception.Message));
            }
        }

        [HttpPost("{id:guid}/result/microbiology/reopen")]
        [ProducesResponseType(typeof(ApiResponse<LabExaminationCompletionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Reopen Lab Microbiology Result", Description = "Membuka kembali penulisan hasil Mikrobiologi sebelum rilis", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("LabExamination", "Update")]
        public async Task<IActionResult> ReopenMicrobiologyResult(
            Guid id,
            [FromBody] LabReopenRequest request,
            CancellationToken cancellationToken = default)
        {
            // Reopen SEBELUM rilis adalah penyuntingan biasa, bukan koreksi hasil terrilis —
            // ia nol menyentuh S6 maupun DEC-LAB-014 (LAB-DEC-097).
            try
            {
                var result = await _labExaminationService.ReopenMicrobiologyResultAsync(id, request, cancellationToken);

                return Ok(ApiResponse<LabExaminationCompletionResponse>.Ok(
                    result, "Penulisan hasil dibuka kembali."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
            catch (LabExaminationValidationException exception)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity, exception.Message));
            }
        }

        [HttpPut("{id:guid}/result/microbiology/consultation")]
        [ProducesResponseType(typeof(ApiResponse<LabExaminationCompletionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Record Lab Result Consultation", Description = "Mencatat fakta konsultasi hasil", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("LabExamination", "Update")]
        public async Task<IActionResult> RecordConsultation(
            Guid id,
            [FromBody] LabConsultationRequest request,
            CancellationToken cancellationToken = default)
        {
            // Endpoint tersendiri, bukan ikut PUT /result: hasil diisi analis hari ini,
            // konsultasi dicatat sesudah berbicara dengan konsultan besok. Menggabungkannya
            // memaksa pemanggil mengirim ulang seluruh isolat hanya untuk menambah satu
            // tanggal — dan setiap pengiriman ulang adalah kesempatan isolat tertimpa.
            try
            {
                var result = await _labExaminationService.RecordConsultationAsync(id, request, cancellationToken);

                return Ok(ApiResponse<LabExaminationCompletionResponse>.Ok(
                    result, "Fakta konsultasi berhasil dicatat."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
            catch (LabExaminationValidationException exception)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity, exception.Message));
            }
        }

        // =============================================================
        // Pengisian hasil Mikrobiologi berstruktur — BE-LAB-48, slice S4b
        //
        // Satu PUT utuh, bukan endpoint terpisah per isolat: isolat dan kepekaannya adalah
        // ISI sebuah hasil, dan endpoint terpisah akan membuat separuh hasil tersimpan tanpa
        // separuh lainnya.
        //
        // MENGISI saja. Nol memvalidasi, nol merilis, nol menandai kritis (INV-28 dipersempit
        // LAB-DEC-103, tetapi penandaannya sendiri milik S5).
        // =============================================================

        [HttpPut("{id:guid}/result/microbiology")]
        [ProducesResponseType(typeof(ApiResponse<LabMicrobiologyResultResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Set Lab Microbiology Result", Description = "Mengisi hasil Mikrobiologi beserta isolat dan antibiogramnya", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("LabExamination", "Update")]
        public async Task<IActionResult> SetMicrobiologyResult(
            Guid id,
            [FromBody] LabMicrobiologyResultRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _labMicrobiologyResultService.SetResultAsync(id, request, cancellationToken);

                return Ok(ApiResponse<LabMicrobiologyResultResponse>.Ok(
                    result, "Hasil Mikrobiologi berhasil disimpan."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
            catch (LabMicrobiologyResultValidationException exception)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity, exception.Message));
            }
            catch (LabSusceptibilityInterpretationException exception)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity, exception.Message));
            }
        }

        [HttpGet("{id:guid}/result/microbiology")]
        [ProducesResponseType(typeof(ApiResponse<LabMicrobiologyResultResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Microbiology Result", Description = "Membaca hasil Mikrobiologi yang sudah diisi", AccessType = AccessTypes.Read, SortOrder = 8)]
        [AccessPermission("LabExamination", "Read")]
        public async Task<IActionResult> GetMicrobiologyResult(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _labMicrobiologyResultService.GetResultAsync(id, cancellationToken);

                return Ok(ApiResponse<LabMicrobiologyResultResponse>.Ok(
                    result, "Hasil Mikrobiologi berhasil diambil."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }

        // =============================================================
        // Pilihan Dokter Konfirmator — BE-LAB-59, LAB-DEC-111, r26 bagian 21.7
        //
        // Membaca, bukan menetapkan. Endpoint ini nol menyimpan apa pun; penetapan
        // konfirmatornya terjadi pada jalur konsultasi yang sudah ada (BE-LAB-54).
        // =============================================================

        [HttpGet("{id:guid}/confirming-doctor-options")]
        [ProducesResponseType(typeof(ApiResponse<LabConfirmingDoctorOptionsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Confirming Doctor Options", Description = "Melihat pilihan dokter konfirmator beserta sumbernya", AccessType = AccessTypes.Read, SortOrder = 6)]
        [AccessPermission("LabExamination", "Read")]
        public async Task<IActionResult> GetConfirmingDoctorOptions(
            Guid id,
            [FromQuery] string? search = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _confirmingDoctorResolver.ResolveAsync(id, search, cancellationToken);

                return Ok(ApiResponse<LabConfirmingDoctorOptionsResponse>.Ok(
                    result,
                    result.OnDutyScheduleAvailable
                        ? "Pilihan dokter konfirmator berhasil diambil."
                        : "Jadwal jaga belum tersedia; seluruh dokter aktif ditampilkan."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }
    }
}
