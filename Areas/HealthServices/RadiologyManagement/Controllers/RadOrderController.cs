using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/radiology-management/rad-orders")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_RADIOLOGY_MANAGEMENT",
        moduleName: "Health Service Radiology Management",
        displayName: "Rad Order",
        AreaName = "HealthServices",
        ControllerName = "RadOrder",
        Description = "Pencatatan order pemeriksaan radiologi",
        SortOrder = 1
    )]
    [Tags("Health Services / Radiology Management / Rad Order")]
    public class RadOrderController : ControllerBase
    {
        private readonly RadOrderService _radOrderService;
        private readonly RadOrderImageService _radOrderImageService;
        private readonly RadOperationalReportService _radOperationalReportService;

        public RadOrderController(
            RadOrderService radOrderService,
            RadOrderImageService radOrderImageService,
            RadOperationalReportService radOperationalReportService)
        {
            _radOrderService = radOrderService;
            _radOrderImageService = radOrderImageService;
            _radOperationalReportService = radOperationalReportService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<RadOrderFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Order", Description = "Melihat daftar pilihan penyaring order radiologi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadOrder", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var hasil = _radOrderService.GetFilterMetadata();

            return Ok(ApiResponse<RadOrderFilterMetadataResponse>.Ok(
                hasil, "Metadata filter order radiologi berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<RadOrderSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Order", Description = "Melihat rekap order radiologi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadOrder", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] RadOrderListQuery query,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _radOrderService.GetSummaryAsync(query, cancellationToken);

            return Ok(ApiResponse<RadOrderSummaryResponse>.Ok(
                hasil, "Rekap order radiologi berhasil diambil."));
        }

        // Pilihan Dokter Pemeriksa dan Petugas Radiologi untuk Detail Pasien v1 (BE-RAD-20). Milik
        // Radiologi, bukan doctors/options SDM yang dijaga KioskRead — pola Lab FE-LAB-50 T2.
        // Aksinya sama dengan endpoint assignment yang memakainya.
        [HttpGet("examiner-doctor-options")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<RadExaminerDoctorOptionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [AccessAction("Update", "Update Rad Order", Description = "Melihat pilihan dokter pemeriksa radiologi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RadOrder", "Update")]
        public async Task<IActionResult> GetExaminerDoctorOptions(
            [FromQuery] RadAssigneeOptionQuery query,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _radOrderService.GetExaminerDoctorOptionsAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<RadExaminerDoctorOptionResponse>>.Ok(
                hasil, "Daftar dokter pemeriksa berhasil diambil."));
        }

        [HttpGet("radiographer-options")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<RadRadiographerOptionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [AccessAction("Update", "Update Rad Order", Description = "Melihat pilihan petugas radiologi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RadOrder", "Update")]
        public async Task<IActionResult> GetRadiographerOptions(
            [FromQuery] RadAssigneeOptionQuery query,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _radOrderService.GetRadiographerOptionsAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<RadRadiographerOptionResponse>>.Ok(
                hasil, "Daftar petugas radiologi berhasil diambil."));
        }

        // Pilihan pemeriksaan untuk Ubah Pemeriksaan (BE-RAD-25). Radiografer tidak memegang
        // Procedure : Read master data, jadi pilihannya disajikan Radiologi sendiri — pola BE-RAD-20.
        [HttpGet("procedure-options")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<RadProcedureOptionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [AccessAction("Update", "Update Rad Order", Description = "Melihat pilihan pemeriksaan radiologi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RadOrder", "Update")]
        public async Task<IActionResult> GetProcedureOptions(
            [FromQuery] RadAssigneeOptionQuery query,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _radOrderService.GetProcedureOptionsAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<RadProcedureOptionResponse>>.Ok(
                hasil, "Daftar pemeriksaan radiologi berhasil diambil."));
        }

        // Booking Radiologi v1 (tombol Tambah Daftar Pasien): cari kunjungan dari No. RM, harga +
        // alat per pemeriksaan, lalu simpan seluruh pemeriksaan sekaligus. Izinnya RadOrder : Create
        // — sama dengan membuat pesanan.
        [HttpGet("booking-encounters")]
        [ProducesResponseType(typeof(ApiResponse<List<RadBookingEncounterResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Rad Order", Description = "Mencari kunjungan pasien untuk booking radiologi", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("RadOrder", "Create")]
        public async Task<IActionResult> GetBookingEncounters(
            [FromQuery] string? medicalRecordNumber,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _radOrderService.GetBookingEncountersAsync(medicalRecordNumber, cancellationToken);

            return Ok(ApiResponse<List<RadBookingEncounterResponse>>.Ok(
                hasil, "Kunjungan pasien berhasil diambil."));
        }

        [HttpGet("booking-price")]
        [ProducesResponseType(typeof(ApiResponse<RadBookingPriceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Create", "Create Rad Order", Description = "Melihat harga dan alat pemeriksaan untuk booking radiologi", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("RadOrder", "Create")]
        public Task<IActionResult> GetBookingPrice(
            [FromQuery] Guid encounterId,
            [FromQuery] Guid procedureId,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.GetBookingPriceAsync(encounterId, procedureId, cancellationToken),
                "Harga pemeriksaan berhasil diambil.");

        [HttpPost("bookings")]
        [ProducesResponseType(typeof(ApiResponse<RadBookingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Rad Order", Description = "Menyimpan booking radiologi", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("RadOrder", "Create")]
        public Task<IActionResult> CreateBooking(
            [FromBody] RadBookingRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.CreateBookingAsync(request, cancellationToken),
                "Booking radiologi berhasil disimpan.");

        // Laporan operasional — menu v1 Laporan dan Pemeriksaan, Setup Tarif Tindakan (BE-RAD-24,
        // RAD-DEC-021). Baca saja; rentang tanggal WIB, bawaan 30 hari, maksimal 366 hari.
        [HttpGet("reports/examination-statistics")]
        [ProducesResponseType(typeof(ApiResponse<RadOperationalReportResponse<RadExaminationStatisticRow>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Order", Description = "Melihat laporan statistik pemeriksaan radiologi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadOrder", "Read")]
        public Task<IActionResult> GetExaminationStatistics([FromQuery] RadOperationalReportQuery query, CancellationToken cancellationToken = default) =>
            Execute(() => _radOperationalReportService.GetExaminationStatisticsAsync(query, cancellationToken),
                "Statistik pemeriksaan radiologi berhasil diambil.");

        [HttpGet("reports/referrers")]
        [ProducesResponseType(typeof(ApiResponse<RadOperationalReportResponse<RadReferrerRow>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Order", Description = "Melihat laporan dokter pengirim radiologi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadOrder", "Read")]
        public Task<IActionResult> GetReferrers([FromQuery] RadOperationalReportQuery query, CancellationToken cancellationToken = default) =>
            Execute(() => _radOperationalReportService.GetReferrersAsync(query, cancellationToken),
                "Laporan dokter pengirim berhasil diambil.");

        [HttpGet("reports/company-patients")]
        [ProducesResponseType(typeof(ApiResponse<RadOperationalReportResponse<RadOrderReportRow>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Order", Description = "Melihat laporan pasien perusahaan radiologi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadOrder", "Read")]
        public Task<IActionResult> GetCompanyPatients([FromQuery] RadOperationalReportQuery query, CancellationToken cancellationToken = default) =>
            Execute(() => _radOperationalReportService.GetCompanyPatientsAsync(query, cancellationToken),
                "Laporan pasien perusahaan berhasil diambil.");

        [HttpGet("reports/daily")]
        [ProducesResponseType(typeof(ApiResponse<RadOperationalReportResponse<RadDailyExaminationRow>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Order", Description = "Melihat pemeriksaan radiologi harian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadOrder", "Read")]
        public Task<IActionResult> GetDaily([FromQuery] RadOperationalReportQuery query, CancellationToken cancellationToken = default) =>
            Execute(() => _radOperationalReportService.GetDailyAsync(query, cancellationToken),
                "Pemeriksaan harian berhasil diambil.");

        [HttpGet("reports/per-item")]
        [ProducesResponseType(typeof(ApiResponse<RadOperationalReportResponse<RadOrderReportRow>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Order", Description = "Melihat pemeriksaan radiologi per item", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadOrder", "Read")]
        public Task<IActionResult> GetPerItem([FromQuery] RadOperationalReportQuery query, CancellationToken cancellationToken = default) =>
            Execute(() => _radOperationalReportService.GetPerItemAsync(query, cancellationToken),
                "Pemeriksaan per item berhasil diambil.");

        [HttpGet("reports/procedure-tariffs")]
        [ProducesResponseType(typeof(ApiResponse<List<RadProcedureTariffRow>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Order", Description = "Melihat tarif tindakan radiologi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadOrder", "Read")]
        public async Task<IActionResult> GetProcedureTariffs(CancellationToken cancellationToken = default)
        {
            var hasil = await _radOperationalReportService.GetProcedureTariffsAsync(cancellationToken);
            return Ok(ApiResponse<List<RadProcedureTariffRow>>.Ok(hasil, "Tarif tindakan radiologi berhasil diambil."));
        }

        // Beranda Radiologi v1 (BE-RAD-19, RAD-EVD-001 bagian 2.1): angka 30 hari WIB terakhir dan
        // lima pesanan terbaru. Baca saja.
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(ApiResponse<RadDashboardResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [AccessAction("Read", "Read Rad Order", Description = "Melihat Beranda Radiologi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadOrder", "Read")]
        public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken = default)
        {
            var hasil = await _radOrderService.GetDashboardAsync(cancellationToken: cancellationToken);

            return Ok(ApiResponse<RadDashboardResponse>.Ok(
                hasil, "Beranda radiologi berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<RadOrderListResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Order", Description = "Melihat daftar order radiologi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadOrder", "Read")]
        // RAD-CONF-001 bagian 8 butir 3. Penyaring pindah ke satu objek query supaya lima
        // kategori daftar pasien radiologi dan delapan kriteria riwayat dilayani dari sini.
        //
        // Pemanggil lama tidak perlu berubah: ?encounterId=, ?sortBy=, dan ?sortDirection=
        // tetap terikat ke properti bernama sama pada RadOrderListQuery.
        public async Task<IActionResult> GetList(
            [FromQuery] RadOrderListQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _radOrderService.GetListAsync(query, cancellationToken);

            return Ok(ApiResponse<List<RadOrderListResponse>>.Ok(
                result, "Daftar order radiologi berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RadOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Rad Order", Description = "Melihat detail order radiologi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadOrder", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _radOrderService.GetDetailAsync(id, cancellationToken);

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Order radiologi tidak ditemukan.",
                    new { Code = RadErrorCodes.OrderNotFound }));
            }

            return Ok(ApiResponse<RadOrderDetailResponse>.Ok(
                result, "Detail order radiologi berhasil diambil."));
        }

        // Pesanan radiologi dan ketersediaan hasilnya untuk satu perawatan rawat inap.
        //
        // BE-RWI-052, api-contract.md bagian 8. Hasil yang belum final ditandai dan TIDAK
        // disajikan sebagai hasil sah — VAL-DOK-30. Tidak ada satu pun baris hasil yang
        // disalin ke Rawat Inap — RUL-DOK-02.
        [HttpGet("episodes/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<RadOrderListResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Order", Description = "Melihat pesanan dan hasil radiologi satu perawatan rawat inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadOrder", "Read")]
        public async Task<IActionResult> GetByEpisode(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var result = await _radOrderService.GetByEpisodeAsync(episodeId, cancellationToken);

            return Ok(ApiResponse<List<RadOrderListResponse>>.Ok(
                result,
                "Pesanan radiologi perawatan rawat inap berhasil diambil."));
        }

        // ISSUE-DOK-001 ISS-06. Operasi create menjawab 201 seperti keluarga endpoint create lain
        // pada repository ini - physician-visits, lab-orders, dan patient-procedures.
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RadOrderDetailResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Create", "Create Rad Order", Description = "Membuat order radiologi", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("RadOrder", "Create")]
        public Task<IActionResult> Create(
            [FromBody] CreateRadOrderRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.CreateAsync(request, cancellationToken),
                "Order radiologi berhasil dibuat.",
                StatusCodes.Status201Created);

        // Dokter pemberi instruksi memverifikasi pesanan radiologi rawat inap yang dibuat perawat —
        // `BE-RWI-104`, api-contract 0.6.0 bagian 12.12.
        //
        // Hak akses baru `RadOrder : Verify`, disetujui pemilik modul lewat `RWI-DEC-153`.
        // Jawaban: `200`; `403` bukan pemberi instruksi; `404`; `409` sudah
        // diverifikasi atau tidak memerlukan verifikasi.
        [HttpPut("{id:guid}/verify-instruction")]
        [ProducesResponseType(typeof(ApiResponse<RadOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Verify", "Verify Rad Order Instruction", Description = "Dokter pemberi instruksi memverifikasi pesanan radiologi yang dibuat perawat", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("RadOrder", "Verify")]
        public Task<IActionResult> VerifyInstruction(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.VerifyInstructionAsync(id, cancellationToken),
                "Instruksi order radiologi berhasil diverifikasi.");

        // Pesanan radiologi yang menunggu verifikasi dokter login — `BE-RWI-104`.
        [HttpGet("instruction-verification-worklist")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<RadOrderInstructionVerificationItemResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [AccessAction("Read", "Read Rad Order", Description = "Melihat pesanan radiologi yang menunggu verifikasi dokter login", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadOrder", "Read")]
        public Task<IActionResult> GetInstructionVerificationWorklist(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.GetInstructionVerificationWorklistAsync(pageNumber, pageSize, cancellationToken),
                "Daftar tunggu verifikasi instruksi radiologi berhasil diambil.");

        [HttpPut("{id:guid}/accept")]
        [AccessAction("Process", "Process Rad Order", Description = "Menerima order radiologi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RadOrder", "Process")]
        public Task<IActionResult> Accept(
            Guid id, [FromBody] RadOrderTransitionRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.AcceptAsync(id, request, cancellationToken),
                "Order radiologi berhasil diterima.");

        [HttpPut("{id:guid}/schedule")]
        [AccessAction("Schedule", "Schedule Rad Order", Description = "Menjadwalkan order radiologi", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("RadOrder", "Schedule")]
        public Task<IActionResult> Schedule(
            Guid id, [FromBody] RadOrderTransitionRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.ScheduleAsync(id, request, cancellationToken),
                "Order radiologi berhasil dijadwalkan.");

        [HttpPut("{id:guid}/start")]
        [AccessAction("Process", "Process Rad Order", Description = "Memulai pengerjaan order radiologi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RadOrder", "Process")]
        public Task<IActionResult> Start(
            Guid id, [FromBody] RadOrderTransitionRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.StartAsync(id, request, cancellationToken),
                "Order radiologi mulai dikerjakan.");

        [HttpPut("{id:guid}/complete")]
        [AccessAction("Process", "Process Rad Order", Description = "Menyelesaikan order radiologi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RadOrder", "Process")]
        public Task<IActionResult> Complete(
            Guid id, [FromBody] RadOrderTransitionRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.CompleteAsync(id, request, cancellationToken),
                "Order radiologi berhasil diselesaikan.");

        [HttpPut("{id:guid}/hold")]
        [AccessAction("Hold", "Hold Rad Order", Description = "Menahan order radiologi", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("RadOrder", "Hold")]
        public Task<IActionResult> Hold(
            Guid id, [FromBody] RadOrderTransitionRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.HoldAsync(id, request, cancellationToken),
                "Order radiologi berhasil ditahan.");

        [HttpPut("{id:guid}/resume")]
        [AccessAction("Hold", "Hold Rad Order", Description = "Melanjutkan order radiologi yang ditahan", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("RadOrder", "Hold")]
        public Task<IActionResult> Resume(
            Guid id, [FromBody] RadOrderTransitionRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.ResumeAsync(id, request, cancellationToken),
                "Order radiologi berhasil dilanjutkan.");

        [HttpPut("{id:guid}/reject")]
        [AccessAction("Update", "Update Rad Order", Description = "Menolak order radiologi", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("RadOrder", "Update")]
        public Task<IActionResult> Reject(
            Guid id, [FromBody] RadOrderTransitionRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.RejectAsync(id, request, cancellationToken),
                "Order radiologi berhasil ditolak.");

        [HttpPut("{id:guid}/cancel")]
        [AccessAction("Cancel", "Cancel Rad Order", Description = "Membatalkan order radiologi", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("RadOrder", "Cancel")]
        public Task<IActionResult> Cancel(
            Guid id, [FromBody] RadOrderTransitionRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.CancelAsync(id, request, cancellationToken),
                "Order radiologi berhasil dibatalkan.");

        // Daftar kerja petugas pada satu alat — RAD-DEC-012.
        //
        // Diletakkan pada controller pesanan, bukan controller study, dan itu disengaja: daftar
        // kerja dimulai dari pesanan yang sudah diterima tetapi belum tentu punya study. Kalau
        // diletakkan di controller study, pekerjaan yang belum direncanakan sama sekali tidak
        // akan muncul — padahal justru itu yang paling perlu dikerjakan.
        [HttpGet("worklist")]
        [ProducesResponseType(typeof(ApiResponse<List<RadWorklistItemResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Rad Order", Description = "Melihat daftar kerja petugas pada satu alat pencitraan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadOrder", "Read")]
        public Task<IActionResult> GetWorklist(
            [FromQuery] Guid? modalityId = null,
            [FromQuery] DateTime? date = null,
            [FromQuery] RadOrderStatus? status = null,
            CancellationToken cancellationToken = default) =>
            Execute(
                () => _radOrderService.GetWorklistAsync(modalityId, date, status, cancellationToken),
                "Daftar kerja berhasil diambil.");

        // Mengubah penanda cito setelah pesanan dibuat. POST, bukan PATCH: perubahannya masuk
        // riwayat dan punya pelaku, sehingga ia perintah — bukan suntingan atribut.
        //
        // Kontrak RAD-API-001 menuliskannya sebagai PUT. Bentuk yang dipakai mengikuti kontrak;
        // selisihnya terhadap aturan verb pada transaction-endpoint-standard.md dicatat pada
        // laporan BE-RAD-13.
        [HttpPut("{id:guid}/urgency")]
        [ProducesResponseType(typeof(ApiResponse<RadOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Rad Order", Description = "Mengubah penanda cito pesanan radiologi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RadOrder", "Update")]
        public Task<IActionResult> SetUrgency(
            Guid id,
            [FromBody] RadOrderUrgencyRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.SetUrgencyAsync(id, request, cancellationToken),
                request is { IsUrgent: true }
                    ? "Pesanan berhasil ditandai cito."
                    : "Penanda cito berhasil dicabut.");

        // Petugas Radiologi dan Dokter Pemeriksa — Detail Pasien Radiologi v1 (BE-RAD-20,
        // RAD-EVD-001 bagian 7.1). Tercatat di riwayat sebagai Order.Assign.
        [HttpPut("{id:guid}/assignment")]
        [ProducesResponseType(typeof(ApiResponse<RadOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Rad Order", Description = "Menetapkan petugas radiologi dan dokter pemeriksa", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RadOrder", "Update")]
        public Task<IActionResult> Assign(
            Guid id,
            [FromBody] RadOrderAssignmentRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.AssignAsync(id, request, cancellationToken),
                "Petugas dan dokter pemeriksa berhasil disimpan.");

        // Ubah Pemeriksaan dan Cetak Film — Detail Pasien v1 (BE-RAD-25, RAD-DEC-021).
        [HttpPut("{id:guid}/procedure")]
        [ProducesResponseType(typeof(ApiResponse<RadOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Rad Order", Description = "Mengubah pemeriksaan pesanan radiologi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RadOrder", "Update")]
        public Task<IActionResult> ChangeProcedure(
            Guid id,
            [FromBody] RadOrderChangeProcedureRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.ChangeProcedureAsync(id, request, cancellationToken),
                "Pemeriksaan pesanan berhasil diubah.");

        // Tambah Baris — Detail Pasien v1: pemeriksaan tambahan pada booking yang sama. Izinnya
        // RadOrder : Update seperti Ubah Pemeriksaan, karena dikerjakan petugas Radiologi pada
        // booking yang sudah ada, bukan pesanan baru dokter pengirim.
        [HttpPost("{id:guid}/examinations")]
        [ProducesResponseType(typeof(ApiResponse<RadOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Rad Order", Description = "Menambah pemeriksaan pada booking radiologi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RadOrder", "Update")]
        public Task<IActionResult> AddExamination(
            Guid id,
            [FromBody] RadOrderAddExaminationRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.AddExaminationAsync(id, request, cancellationToken),
                "Pemeriksaan berhasil ditambahkan.");

        [HttpPut("{id:guid}/film-print")]
        [ProducesResponseType(typeof(ApiResponse<RadOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Rad Order", Description = "Mengatur cetak film pesanan radiologi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RadOrder", "Update")]
        public Task<IActionResult> SetFilmPrint(
            Guid id,
            [FromBody] RadOrderFilmPrintRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.SetFilmPrintAsync(id, request, cancellationToken),
                request is { PrintFilm: true } ? "Cetak Film dicentang." : "Cetak Film dicabut.");

        // Unggah Gambar — Form Hasil v1 (BE-RAD-22, RAD-EVD-001 bagian 7.1). Berkas di penyimpanan
        // privat; isinya hanya dapat dibaca lewat /content yang berizin.
        [HttpGet("{id:guid}/images")]
        [ProducesResponseType(typeof(ApiResponse<List<RadOrderImageResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Rad Order", Description = "Melihat gambar radiologi pesanan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadOrder", "Read")]
        public Task<IActionResult> GetImages(Guid id, CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderImageService.GetListAsync(id, cancellationToken),
                "Gambar radiologi berhasil diambil.");

        [HttpPost("{id:guid}/images")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(220L * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 220L * 1024 * 1024)]
        [ProducesResponseType(typeof(ApiResponse<List<RadOrderImageResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Rad Order", Description = "Mengunggah gambar radiologi pesanan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RadOrder", "Update")]
        public Task<IActionResult> UploadImages(
            Guid id,
            [FromForm] List<IFormFile> files,
            [FromForm] Guid? radStudyId,
            CancellationToken cancellationToken = default)
        {
            var unggahan = (files ?? new List<IFormFile>())
                .Select(f => new RadOrderImageService.UploadFile(f.FileName, f.Length, f.OpenReadStream))
                .ToList();

            return Execute(() => _radOrderImageService.UploadAsync(id, radStudyId, unggahan, cancellationToken),
                "Gambar radiologi berhasil diunggah.");
        }

        [HttpGet("{id:guid}/images/{imageId:guid}/content")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Rad Order", Description = "Membuka gambar radiologi pesanan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadOrder", "Read")]
        public async Task<IActionResult> GetImageContent(Guid id, Guid imageId, CancellationToken cancellationToken = default)
        {
            var hasil = await _radOrderImageService.OpenAsync(id, imageId, cancellationToken);

            if (!hasil.IsSuccess || hasil.Value == null)
            {
                return hasil.Kind == RadOperationResultKind.NotFound
                    ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, hasil.ErrorMessage ?? "Gambar tidak ditemukan.", new { Code = hasil.ErrorCode }))
                    : BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, hasil.ErrorMessage ?? "Permintaan tidak valid.", new { Code = hasil.ErrorCode }));
            }

            Response.Headers["Cache-Control"] = "private, no-store";
            return File(hasil.Value.Stream, hasil.Value.ContentType, enableRangeProcessing: false);
        }

        [HttpDelete("{id:guid}/images/{imageId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<RadOrderImageResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Rad Order", Description = "Menghapus gambar radiologi pesanan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RadOrder", "Update")]
        public Task<IActionResult> DeleteImage(Guid id, Guid imageId, CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderImageService.DeleteAsync(id, imageId, cancellationToken),
                "Gambar radiologi berhasil dihapus.");

        /// <summary>
        /// Menjalankan satu tindakan dan memetakan hasilnya menjadi status HTTP.
        ///
        /// Pemetaan ini yang membuat perbedaan jenis penolakan sampai ke pemanggil.
        /// `SafetyBlocked` dan `PolicyNotConfigured` sama-sama menjadi <c>422</c>, bukan
        /// <c>400</c>: permintaannya sendiri sah, yang belum terpenuhi adalah prasyaratnya.
        /// Kode galatnya tetap berbeda supaya layar dapat menuntun ke tindakan yang benar.
        /// </summary>
        private async Task<IActionResult> Execute<T>(
            Func<Task<RadOperationResult<T>>> action,
            string successMessage,
            int successStatusCode = StatusCodes.Status200OK)
        {
            try
            {
                var result = await action();

                return result.Kind switch
                {
                    RadOperationResultKind.Success =>
                        StatusCode(successStatusCode, ApiResponse<T>.Ok(result.Value, successMessage)),

                    RadOperationResultKind.NotFound =>
                        NotFound(ApiResponse<object>.Fail(
                            StatusCodes.Status404NotFound,
                            result.ErrorMessage ?? "Data tidak ditemukan.",
                            new { Code = result.ErrorCode })),

                    RadOperationResultKind.Conflict =>
                        Conflict(ApiResponse<object>.Fail(
                            StatusCodes.Status409Conflict,
                            result.ErrorMessage ?? "Terjadi konflik.",
                            new { Code = result.ErrorCode })),

                    // BE-RWI-104. Sebelumnya tidak ada jalur RadOrderService yang menghasilkan
                    // Forbidden, sehingga penambahan cabang ini tidak mengubah jawaban endpoint lama.
                    RadOperationResultKind.Forbidden =>
                        StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(
                            StatusCodes.Status403Forbidden,
                            result.ErrorMessage ?? "Anda tidak berwenang.",
                            new { Code = result.ErrorCode })),

                    RadOperationResultKind.SafetyBlocked or
                    RadOperationResultKind.PolicyNotConfigured =>
                        UnprocessableEntity(ApiResponse<object>.Fail(
                            StatusCodes.Status422UnprocessableEntity,
                            result.ErrorMessage ?? "Prasyarat belum terpenuhi.",
                            new { Code = result.ErrorCode })),

                    _ => BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        result.ErrorMessage ?? "Permintaan tidak valid.",
                        new { Code = result.ErrorCode })),
                };
            }
            catch (RadConcurrencyException exception)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict,
                    exception.Message,
                    new { Code = RadErrorCodes.ConcurrencyConflict }));
            }
        }
    }
}
