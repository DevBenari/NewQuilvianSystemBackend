using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/payroll-management/workforce-profiles/{workforceProfileId:guid}/tax-profile")]
    // Alias kompatibilitas: rute di bawah ini adalah rute sebelum Payroll dipisah dari HRD; dihapus bertahap setelah seluruh consumer berpindah.
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/tax-profile")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_PAYROLL_MANAGEMENT",
        moduleName: "Human Resource Payroll Management",
        displayName: "Workforce Tax Profile",
        AreaName = "Corporate",
        ControllerName = "WorkforceTaxProfile",
        Description = "Corporate human resource workforce tax profile",
        SortOrder = 2
    )]
    [Tags("Corporate / Payroll Management / Tax Profile")]
    public class WfpTaxController : ControllerBase
    {
        private static readonly HashSet<string> AllowedTaxMethods = new(StringComparer.OrdinalIgnoreCase)
        {
            "Gross",
            "GrossUp",
            "Net"
        };

        private static readonly HashSet<string> AllowedTaxStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "TK/0", "TK/1", "TK/2", "TK/3",
            "K/0", "K/1", "K/2", "K/3",
            "K/I/0", "K/I/1", "K/I/2", "K/I/3"
        };

        private const string LogCategory = "Corporate.HumanResource.PayrollManagement";
        private const string Resource = "WorkforceTaxProfile";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly AccessPermissionService _accessPermissionService;
        private readonly PayrollStatutoryPolicyResolver _policyResolver;

        public WfpTaxController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            AccessPermissionService accessPermissionService,
            PayrollStatutoryPolicyResolver policyResolver)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _accessPermissionService = accessPermissionService;
            _policyResolver = policyResolver;
        }

        /// <summary>Nominal PPh manual bersifat sensitif: hanya pemilik izin ViewAmount yang melihat dan menuliskannya.</summary>
        private Task<bool> CanViewAmountAsync() => _accessPermissionService.HasAccessAsync(User, Resource, "ViewAmount");

        /// <summary>
        /// Persetujuan nominal PPh manual adalah izin tersendiri (ApproveManualAmount): menetapkan nominal (Update, ViewAmount) bukan
        /// persetujuan. Diisi sebelum memetakan respons; hanya bantuan tampilan, setiap keputusan diperiksa ulang di Backend.
        /// </summary>
        private bool _canDecideManualAmount;

        private async Task LoadDecisionCapabilityAsync() =>
            _canDecideManualAmount = await _accessPermissionService.HasAccessAsync(User, Resource, "ApproveManualAmount");

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WfpTaxFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Tax Profile", Description = "Melihat metadata filter profil pajak workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceTaxProfile", "Read")]
        public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken)
        {
            var availability = await _policyResolver.GetAvailabilityAsync(DateTime.UtcNow, cancellationToken);
            var result = new WfpTaxFilterMetadataResponse
            {
                DefaultFilter = new WfpTaxDefaultFilterResponse(),
                TaxStatusOptions = AllowedTaxStatuses
                    .OrderBy(x => x)
                    .Select(x => new WfpTaxStringOptionResponse
                    {
                        Value = x,
                        Label = x
                    })
                    .ToList(),
                TaxMethodOptions = AllowedTaxMethods
                    .OrderBy(x => x)
                    .Select(x => new WfpTaxStringOptionResponse
                    {
                        Value = x,
                        Label = x == "GrossUp" ? "Gross Up" : x,
                        // Metode GrossUp belum memiliki formula yang sah: dikenali, tetapi belum dapat dipakai bersama nominal manual.
                        IsSupported = x != TaxMethodKinds.GrossUp,
                        Note = x == TaxMethodKinds.GrossUp ? PayrollTaxInsuranceRules.GrossUpUnavailable : null
                    })
                    .ToList(),
                TaxArticleOptions = new List<WfpTaxStringOptionResponse>
                {
                    new() { Value = TaxArticles.Pph21, Label = "PPh Pasal 21", Note = "Subjek pajak dalam negeri." },
                    new() { Value = TaxArticles.Pph26, Label = "PPh Pasal 26", Note = "Hanya subjek pajak luar negeri yang memenuhi ketentuan; bukan otomatis karena berkewarganegaraan asing." }
                },
                RecipientTaxCategoryOptions = new List<WfpTaxStringOptionResponse>
                {
                    new() { Value = TaxRecipientCategories.PermanentEmployee, Label = "Pegawai Tetap" },
                    new() { Value = TaxRecipientCategories.NonPermanentEmployee, Label = "Pegawai Tidak Tetap", Note = "Perhitungan otomatis belum tersedia (harian/mingguan/satuan); gunakan Nominal Manual Disetujui atau Tidak Berlaku dengan dasar yang sah." },
                    new() { Value = TaxRecipientCategories.NonEmployeeExpert, Label = "Bukan Pegawai / Tenaga Ahli", Note = "Dokter tidak otomatis pegawai tetap; tentukan sesuai hubungan kerja aktual." }
                },
                CalculationModeOptions = new List<WfpTaxStringOptionResponse>
                {
                    new()
                    {
                        Value = TaxCalculationModes.AutoRegulatory,
                        Label = "Otomatis Sesuai Peraturan",
                        IsSupported = true,
                        Note = "Dihitung dari kebijakan tarif PPh berversi yang disahkan Finance. Tanpa kebijakan yang berlaku pada periode, payroll run menahan perhitungan (Menunggu Verifikasi Kebijakan). Tidak tersedia untuk pegawai tidak tetap dan metode GrossUp."
                    },
                    new()
                    {
                        Value = TaxCalculationModes.ApprovedManualAmount,
                        Label = "Nominal Manual Disetujui",
                        Note = "Wajib dasar penetapan dan hanya untuk pemegang izin melihat nominal."
                    },
                    new()
                    {
                        Value = TaxCalculationModes.ExemptOrNotApplicable,
                        Label = "Tidak Berlaku / Dikecualikan",
                        Note = "Wajib alasan dan dasar aturan. Bukan karena hasil hitung nol."
                    }
                },
                TaxPolicyApproved = availability.TaxApproved,
                TaxPolicyVersion = availability.TaxVersion,
                TaxPolicyStatusLabel = availability.TaxApproved ? "Kebijakan tarif PPh disahkan" : StatutoryPolicyLabels.AwaitingVerification,
                SortOptions = new List<WfpTaxSortOptionResponse>
                {
                    new() { Value = "taxStatus", Label = "Status pajak" },
                    new() { Value = "taxMethod", Label = "Metode pajak" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal mulai berlaku" },
                    new() { Value = "isActive", Label = "Status aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WfpTaxFilterMetadataResponse>.Ok(
                result,
                "Metadata filter profil pajak workforce berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WfpTaxSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Tax Profile", Description = "Melihat ringkasan profil pajak workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceTaxProfile", "Read")]
        public async Task<IActionResult> GetSummary(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            var query = _dbContext.Set<PrlTax>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var result = new WfpTaxSummaryResponse
            {
                TotalTaxProfile = await query.CountAsync(cancellationToken),
                ActiveTaxProfile = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveTaxProfile = await query.CountAsync(x => !x.IsActive, cancellationToken),
                NpwpRegisteredProfile = await query.CountAsync(x => x.IsNpwpRegistered, cancellationToken),
                TaxResidentProfile = await query.CountAsync(x => x.IsTaxResident, cancellationToken),
                PreviousEmployerProfile = await query.CountAsync(x => x.HasPreviousEmployer, cancellationToken),
                EmployerBorneTaxProfile = await query.CountAsync(x => x.IsEmployerBorneTax, cancellationToken)
            };

            // Konfigurasi dinilai pada tanggal hari ini oleh aturan yang sama dengan mesin hitung (bukan perbandingan nominal).
            var today = DateTime.UtcNow.Date;
            var rows = await query.ToListAsync(cancellationToken);
            result.ConfiguredProfile = rows.Count(x => PayrollTaxInsuranceRules.AssessTax(PayrollTaxConfigFacts.From(x), 0m, today, today).IsConfigured);
            result.NotConfiguredProfile = rows.Count - result.ConfiguredProfile;

            return Ok(ApiResponse<WfpTaxSummaryResponse>.Ok(
                result,
                "Ringkasan profil pajak workforce berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WfpTaxResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Tax Profile", Description = "Melihat data profil pajak workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceTaxProfile", "Read")]
        public async Task<IActionResult> GetTaxProfiles(
            Guid workforceProfileId,
            [FromQuery] string? taxStatus,
            [FromQuery] string? taxMethod,
            [FromQuery] bool? isNpwpRegistered,
            [FromQuery] bool? isTaxResident,
            [FromQuery] bool? hasPreviousEmployer,
            [FromQuery] bool? isEmployerBorneTax,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyFilter(
                BuildBaseQuery(workforceProfileId),
                taxStatus,
                taxMethod,
                isNpwpRegistered,
                isTaxResident,
                hasPreviousEmployer,
                isEmployerBorneTax,
                isActive,
                search);

            var totalData = await query.CountAsync(cancellationToken);
            var rows = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var includeAmounts = await CanViewAmountAsync();
            await LoadDecisionCapabilityAsync();
            var result = new PagedResult<WfpTaxResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = rows.Select(x => MapResponse(x, includeAmounts)).ToList()
            };

            return Ok(ApiResponse<PagedResult<WfpTaxResponse>>.Ok(
                result,
                "Data profil pajak workforce berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpTaxDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Tax Profile", Description = "Melihat detail profil pajak workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceTaxProfile", "Read")]
        public async Task<IActionResult> GetTaxProfileById(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil pajak workforce tidak ditemukan."));
            }

            await LoadDecisionCapabilityAsync();
            var result = MapDetailResponse(entity, await CanViewAmountAsync());

            return Ok(ApiResponse<WfpTaxDetailResponse>.Ok(
                result,
                "Detail profil pajak workforce berhasil diambil."));
        }

        /// <summary>
        /// Pintu izin nominal (pola ViewAmount di repo): endpoint ini sengaja ada agar kemampuan "melihat nominal PPh manual" muncul di layar
        /// Akses Role. Pemilik izin ini juga melihat dan menuliskan nominal pada detail; tanpa izin, nominalnya null.
        /// </summary>
        [HttpGet("{id:guid}/amount")]
        [ProducesResponseType(typeof(ApiResponse<WfpTaxDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("ViewAmount", "View Workforce Tax Profile Amount", Description = "Melihat nominal PPh manual yang disetujui pada profil pajak workforce", AccessType = AccessTypes.Read, SortOrder = 5)]
        [AccessPermission("WorkforceTaxProfile", "ViewAmount")]
        public async Task<IActionResult> GetTaxProfileAmount(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil pajak workforce tidak ditemukan."));
            }

            await LoadDecisionCapabilityAsync();
            return Ok(ApiResponse<WfpTaxDetailResponse>.Ok(
                MapDetailResponse(entity, includeAmounts: true),
                "Nominal profil pajak workforce berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WfpTaxDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Workforce Tax Profile", Description = "Membuat profil pajak workforce", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceTaxProfile", "Create")]
        public async Task<IActionResult> CreateTaxProfile(
            Guid workforceProfileId,
            [FromBody] CreateWfpTaxRequest request,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            if (await _dbContext.Set<PrlTax>().AnyAsync(
                    x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete,
                    cancellationToken))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Profil pajak untuk workforce ini sudah tersedia."));
            }

            var canViewAmount = await CanViewAmountAsync();
            if (request.ApprovedManualAmount.HasValue && !canViewAmount)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Menetapkan nominal PPh manual membutuhkan izin melihat nominal (ViewAmount)."));
            }

            var validation = await ValidateRequestAsync(request, null, request.ApprovedManualAmount, cancellationToken);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data profil pajak tidak valid."));
            }

            var entity = new PrlTax
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            ApplyRequest(entity, request, request.ApprovedManualAmount);

            _dbContext.Set<PrlTax>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Audit: identitas, konfigurasi, dan aktor. Nominal manual TIDAK dicatat; hanya penanda ada atau tidaknya.
            await _loggerService.AuditAsync(
                LogCategory,
                "WorkforceTaxProfile.Create",
                "Membuat profil pajak workforce.",
                AuditSnapshot(entity, null));

            return await GetTaxProfileById(workforceProfileId, entity.Id, cancellationToken);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpTaxDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Workforce Tax Profile", Description = "Mengubah profil pajak workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceTaxProfile", "Update")]
        public async Task<IActionResult> UpdateTaxProfile(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpTaxRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<PrlTax>()
                .FirstOrDefaultAsync(
                    x => x.Id == id &&
                         x.WorkforceProfileId == workforceProfileId &&
                         !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil pajak workforce tidak ditemukan."));
            }

            var canViewAmount = await CanViewAmountAsync();
            if (request.ApprovedManualAmount.HasValue && !canViewAmount)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Menetapkan nominal PPh manual membutuhkan izin melihat nominal (ViewAmount)."));
            }

            // Pemanggil tanpa ViewAmount tidak pernah melihat nominal sehingga tidak dapat mengirimnya kembali: nominal yang sudah
            // disetujui dipertahankan selama skema tetap APPROVED_MANUAL_AMOUNT, bukan terhapus diam-diam.
            var requestedMode = PayrollTaxInsuranceRules.NormalizeCode(request.CalculationMode);
            var effectiveManualAmount = request.ApprovedManualAmount;
            if (!canViewAmount && requestedMode == TaxCalculationModes.ApprovedManualAmount)
                effectiveManualAmount = entity.ApprovedManualAmount;

            var validation = await ValidateRequestAsync(request, id, effectiveManualAmount, cancellationToken);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data profil pajak tidak valid."));
            }

            var before = AuditSnapshot(entity, null);
            ApplyRequest(entity, request, effectiveManualAmount);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "WorkforceTaxProfile.Update",
                "Mengubah profil pajak workforce.",
                AuditSnapshot(entity, before));

            return await GetTaxProfileById(workforceProfileId, entity.Id, cancellationToken);
        }

        /// <summary>
        /// Menyetujui nominal PPh manual. Izin tersendiri (ApproveManualAmount) dan bukan pengganti Update/ViewAmount: pemberi persetujuan
        /// harus melihat nominal yang disetujuinya (ViewAmount) dan tidak boleh orang yang menetapkannya. Nominal yang belum disetujui
        /// tidak dipakai payroll.
        /// </summary>
        [HttpPost("{id:guid}/manual-amount/approve")]
        [ProducesResponseType(typeof(ApiResponse<WfpTaxDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("ApproveManualAmount", "Approve Workforce Tax Manual Amount", Description = "Menyetujui atau menolak nominal PPh manual yang ditetapkan HRD; harus berbeda dari penetap dan membutuhkan izin melihat nominal", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("WorkforceTaxProfile", "ApproveManualAmount")]
        public Task<IActionResult> ApproveManualAmount(
            Guid workforceProfileId,
            Guid id,
            [FromBody] WfpTaxManualAmountDecisionRequest? request,
            CancellationToken cancellationToken) =>
            DecideManualAmountAsync(workforceProfileId, id, approve: true, request?.Note, cancellationToken);

        [HttpPost("{id:guid}/manual-amount/reject")]
        [ProducesResponseType(typeof(ApiResponse<WfpTaxDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("ApproveManualAmount", "Approve Workforce Tax Manual Amount", Description = "Menyetujui atau menolak nominal PPh manual yang ditetapkan HRD; harus berbeda dari penetap dan membutuhkan izin melihat nominal", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("WorkforceTaxProfile", "ApproveManualAmount")]
        public Task<IActionResult> RejectManualAmount(
            Guid workforceProfileId,
            Guid id,
            [FromBody] WfpTaxManualAmountDecisionRequest? request,
            CancellationToken cancellationToken) =>
            DecideManualAmountAsync(workforceProfileId, id, approve: false, request?.Note, cancellationToken);

        private async Task<IActionResult> DecideManualAmountAsync(
            Guid workforceProfileId,
            Guid id,
            bool approve,
            string? note,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<PrlTax>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, cancellationToken);
            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Profil pajak workforce tidak ditemukan."));
            }

            if (!await CanViewAmountAsync())
            {
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Memutuskan nominal PPh manual membutuhkan izin melihat nominal (ViewAmount)."));
            }

            if (entity.CalculationMode != TaxCalculationModes.ApprovedManualAmount
                || entity.ManualAmountApprovalStatus != ManualAmountApprovalStatuses.Pending)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Hanya nominal PPh manual yang menunggu persetujuan yang dapat diputuskan."));
            }

            var actor = GetCurrentUserId();
            if (entity.ManualAmountSubmittedByUserId.HasValue && entity.ManualAmountSubmittedByUserId.Value == actor)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Penetap nominal tidak boleh menyetujui atau menolak nominal yang ditetapkannya sendiri."));
            }

            var trimmedNote = NormalizeNullableText(note);
            if (!approve && trimmedNote == null)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan penolakan nominal PPh manual wajib diisi."));
            }

            entity.ManualAmountApprovalStatus = approve ? ManualAmountApprovalStatuses.Approved : ManualAmountApprovalStatuses.Rejected;
            entity.ManualAmountDecidedAt = DateTime.UtcNow;
            entity.ManualAmountDecidedByUserId = actor;
            entity.ManualAmountDecisionNote = trimmedNote;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                approve ? "WorkforceTaxProfile.ApproveManualAmount" : "WorkforceTaxProfile.RejectManualAmount",
                approve ? "Menyetujui nominal PPh manual profil pajak workforce." : "Menolak nominal PPh manual profil pajak workforce.",
                AuditSnapshot(entity, null));

            return await GetTaxProfileById(workforceProfileId, entity.Id, cancellationToken);
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Workforce Tax Profile", Description = "Mengubah status profil pajak workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceTaxProfile", "Update")]
        public async Task<IActionResult> UpdateTaxProfileStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpTaxStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<PrlTax>()
                .FirstOrDefaultAsync(
                    x => x.Id == id &&
                         x.WorkforceProfileId == workforceProfileId &&
                         !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil pajak workforce tidak ditemukan."));
            }

            if (request.EffectiveEndDate.HasValue &&
                entity.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.Value.Date < entity.EffectiveStartDate.Value.Date)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate."));
            }

            var before = AuditSnapshot(entity, null);
            entity.IsActive = request.IsActive;
            entity.EffectiveEndDate = PayrollDateNormalizer.ToUtcDate(request.EffectiveEndDate);
            entity.Description = NormalizeNullableText(request.Description) ?? entity.Description;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "WorkforceTaxProfile.UpdateStatus",
                "Mengubah status profil pajak workforce.",
                AuditSnapshot(entity, before));

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status profil pajak workforce berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Delete", "Delete Workforce Tax Profile", Description = "Menghapus profil pajak workforce", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WorkforceTaxProfile", "Delete")]
        public async Task<IActionResult> DeleteTaxProfile(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<PrlTax>()
                .FirstOrDefaultAsync(
                    x => x.Id == id &&
                         x.WorkforceProfileId == workforceProfileId &&
                         !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil pajak workforce tidak ditemukan."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "WorkforceTaxProfile.Delete",
                "Menghapus profil pajak workforce.",
                AuditSnapshot(entity, null));

            return Ok(ApiResponse<object>.Ok(
                null,
                "Profil pajak workforce berhasil dihapus."));
        }

        private IQueryable<PrlTax> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<PrlTax>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);
        }

        private static IQueryable<PrlTax> ApplyFilter(
            IQueryable<PrlTax> query,
            string? taxStatus,
            string? taxMethod,
            bool? isNpwpRegistered,
            bool? isTaxResident,
            bool? hasPreviousEmployer,
            bool? isEmployerBorneTax,
            bool? isActive,
            string? search)
        {
            if (!string.IsNullOrWhiteSpace(taxStatus))
                query = query.Where(x => x.TaxStatus == taxStatus.Trim());

            if (!string.IsNullOrWhiteSpace(taxMethod))
                query = query.Where(x => x.TaxMethod == taxMethod.Trim());

            if (isNpwpRegistered.HasValue)
                query = query.Where(x => x.IsNpwpRegistered == isNpwpRegistered.Value);

            if (isTaxResident.HasValue)
                query = query.Where(x => x.IsTaxResident == isTaxResident.Value);

            if (hasPreviousEmployer.HasValue)
                query = query.Where(x => x.HasPreviousEmployer == hasPreviousEmployer.Value);

            if (isEmployerBorneTax.HasValue)
                query = query.Where(x => x.IsEmployerBorneTax == isEmployerBorneTax.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    (x.NpwpNumber != null && x.NpwpNumber.ToLower().Contains(keyword)) ||
                    x.TaxStatus.ToLower().Contains(keyword) ||
                    x.TaxMethod.ToLower().Contains(keyword) ||
                    (x.TaxOfficeCode != null && x.TaxOfficeCode.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<PrlTax> ApplySorting(
            IQueryable<PrlTax> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = !string.Equals(
                sortDirection?.Trim(),
                "asc",
                StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "createDateTime").Trim().ToLowerInvariant() switch
            {
                "taxstatus" => isDescending
                    ? query.OrderByDescending(x => x.TaxStatus)
                    : query.OrderBy(x => x.TaxStatus),
                "taxmethod" => isDescending
                    ? query.OrderByDescending(x => x.TaxMethod)
                    : query.OrderBy(x => x.TaxMethod),
                "effectivestartdate" => isDescending
                    ? query.OrderByDescending(x => x.EffectiveStartDate)
                    : query.OrderBy(x => x.EffectiveStartDate),
                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),
                _ => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime)
            };
        }

        /// <summary>Ringkasan aman untuk audit: konfigurasi dan aktor, tanpa nominal manual, NPWP, atau data pribadi.</summary>
        private object AuditSnapshot(PrlTax entity, object? before) => new
        {
            entity.Id,
            entity.WorkforceProfileId,
            entity.TaxArticle,
            entity.RecipientTaxCategory,
            entity.CalculationMode,
            entity.TaxMethod,
            entity.IsActive,
            HasApprovedManualAmount = entity.ApprovedManualAmount.HasValue,
            entity.ManualAmountApprovalStatus,
            HasTreatyRate = entity.TreatyRatePercent.HasValue,
            HasDecisionBasis = !string.IsNullOrWhiteSpace(entity.DecisionBasis),
            HasExemptionReason = !string.IsNullOrWhiteSpace(entity.ExemptionReason),
            entity.PolicyVersion,
            entity.EffectiveStartDate,
            entity.EffectiveEndDate,
            Before = before,
            ActorUserId = GetCurrentUserId()
        };

        private WfpTaxResponse MapResponse(PrlTax entity, bool includeAmounts)
        {
            var today = DateTime.UtcNow.Date;
            var assessment = PayrollTaxInsuranceRules.AssessTax(PayrollTaxConfigFacts.From(entity), 0m, today, today);
            var article = PayrollTaxInsuranceRules.NormalizeCode(entity.TaxArticle);
            var category = PayrollTaxInsuranceRules.NormalizeCode(entity.RecipientTaxCategory);

            return new WfpTaxResponse
            {
                TaxArticle = entity.TaxArticle,
                RecipientTaxCategory = entity.RecipientTaxCategory,
                CalculationMode = entity.CalculationMode,
                PolicyVersion = entity.PolicyVersion,
                ExemptionReason = entity.ExemptionReason,
                DecisionBasis = entity.DecisionBasis,
                AmountsVisible = includeAmounts,
                HasApprovedManualAmount = entity.ApprovedManualAmount.HasValue,
                ApprovedManualAmount = includeAmounts ? entity.ApprovedManualAmount : null,
                ManualAmountApprovalStatus = entity.ManualAmountApprovalStatus,
                ManualAmountSubmittedAt = entity.ManualAmountSubmittedAt,
                ManualAmountDecidedAt = entity.ManualAmountDecidedAt,
                ManualAmountDecisionNote = entity.ManualAmountDecisionNote,
                CanDecideManualAmount = _canDecideManualAmount && includeAmounts
                    && entity.ManualAmountApprovalStatus == ManualAmountApprovalStatuses.Pending
                    && entity.ManualAmountSubmittedByUserId != GetCurrentUserId(),
                TreatyRatePercent = entity.TreatyRatePercent,
                TreatyBasis = entity.TreatyBasis,
                ConfigurationStatus = assessment.ConfigurationStatus,
                ConfigurationIssues = assessment.Issues.ToList(),
                IsPtkpRelevant = PayrollTaxInsuranceRules.IsPtkpRelevant(article, category),
                Id = entity.Id,
                WorkforceProfileId = entity.WorkforceProfileId,
                WorkforceProfileCode = entity.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = entity.WorkforceProfile?.DisplayName ?? string.Empty,
                NpwpNumber = entity.NpwpNumber,
                TaxStatus = entity.TaxStatus,
                TaxMethod = entity.TaxMethod,
                TaxCountryCode = entity.TaxCountryCode,
                TaxOfficeCode = entity.TaxOfficeCode,
                IsNpwpRegistered = entity.IsNpwpRegistered,
                IsTaxResident = entity.IsTaxResident,
                HasPreviousEmployer = entity.HasPreviousEmployer,
                IsEmployerBorneTax = entity.IsEmployerBorneTax,
                PreviousEmployerTaxableIncome = entity.PreviousEmployerTaxableIncome,
                PreviousEmployerTaxPaid = entity.PreviousEmployerTaxPaid,
                AnnualNonTaxableIncome = entity.AnnualNonTaxableIncome,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetUserDisplayName(entity.CreateBy)
            };
        }

        private WfpTaxDetailResponse MapDetailResponse(PrlTax entity, bool includeAmounts)
        {
            var response = MapResponse(entity, includeAmounts);

            return new WfpTaxDetailResponse
            {
                TaxArticle = response.TaxArticle,
                RecipientTaxCategory = response.RecipientTaxCategory,
                CalculationMode = response.CalculationMode,
                PolicyVersion = response.PolicyVersion,
                ExemptionReason = response.ExemptionReason,
                DecisionBasis = response.DecisionBasis,
                AmountsVisible = response.AmountsVisible,
                HasApprovedManualAmount = response.HasApprovedManualAmount,
                ApprovedManualAmount = response.ApprovedManualAmount,
                ManualAmountApprovalStatus = response.ManualAmountApprovalStatus,
                ManualAmountSubmittedAt = response.ManualAmountSubmittedAt,
                ManualAmountDecidedAt = response.ManualAmountDecidedAt,
                ManualAmountDecisionNote = response.ManualAmountDecisionNote,
                CanDecideManualAmount = response.CanDecideManualAmount,
                TreatyRatePercent = response.TreatyRatePercent,
                TreatyBasis = response.TreatyBasis,
                ConfigurationStatus = response.ConfigurationStatus,
                ConfigurationIssues = response.ConfigurationIssues,
                IsPtkpRelevant = response.IsPtkpRelevant,
                Id = response.Id,
                WorkforceProfileId = response.WorkforceProfileId,
                WorkforceProfileCode = response.WorkforceProfileCode,
                WorkforceDisplayName = response.WorkforceDisplayName,
                NpwpNumber = response.NpwpNumber,
                TaxStatus = response.TaxStatus,
                TaxMethod = response.TaxMethod,
                TaxCountryCode = response.TaxCountryCode,
                TaxOfficeCode = response.TaxOfficeCode,
                IsNpwpRegistered = response.IsNpwpRegistered,
                IsTaxResident = response.IsTaxResident,
                HasPreviousEmployer = response.HasPreviousEmployer,
                IsEmployerBorneTax = response.IsEmployerBorneTax,
                PreviousEmployerTaxableIncome = response.PreviousEmployerTaxableIncome,
                PreviousEmployerTaxPaid = response.PreviousEmployerTaxPaid,
                AnnualNonTaxableIncome = response.AnnualNonTaxableIncome,
                EffectiveStartDate = response.EffectiveStartDate,
                EffectiveEndDate = response.EffectiveEndDate,
                Description = response.Description,
                IsActive = response.IsActive,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetUserDisplayName(entity.UpdateBy)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            CreateWfpTaxRequest request,
            Guid? currentId,
            decimal? effectiveManualAmount,
            CancellationToken cancellationToken)
        {
            if (!AllowedTaxStatuses.Contains(request.TaxStatus.Trim()))
                return (false, "TaxStatus tidak valid.");

            if (!AllowedTaxMethods.Contains(request.TaxMethod.Trim()))
                return (false, "TaxMethod tidak valid. Gunakan Gross, GrossUp, atau Net.");

            if (string.IsNullOrWhiteSpace(request.TaxCountryCode) ||
                request.TaxCountryCode.Trim().Length != 2)
            {
                return (false, "TaxCountryCode harus menggunakan kode negara dua karakter.");
            }

            if (request.IsNpwpRegistered && string.IsNullOrWhiteSpace(request.NpwpNumber))
                return (false, "Nomor NPWP wajib diisi ketika NPWP terdaftar.");

            if (request.PreviousEmployerTaxableIncome < 0 ||
                request.PreviousEmployerTaxPaid < 0 ||
                request.AnnualNonTaxableIncome < 0)
            {
                return (false, "Nilai perpajakan tidak boleh negatif.");
            }

            if (!request.HasPreviousEmployer &&
                (request.PreviousEmployerTaxableIncome > 0 || request.PreviousEmployerTaxPaid > 0))
            {
                return (false, "Data pemberi kerja sebelumnya hanya boleh diisi jika HasPreviousEmployer aktif.");
            }

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
            {
                return (false, "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate.");
            }

            // Jenis PPh, kategori penerima, metode pajak, dan skema hitung adalah konsep terpisah; kombinasi dan syaratnya dijaga aturan
            // yang sama dengan mesin hitung. Ketiga atribut baru kosong berarti belum dikonfigurasi (kompatibel dengan klien lama).
            var configurationErrors = PayrollTaxInsuranceRules.ValidateTaxConfiguration(
                PayrollTaxInsuranceRules.NormalizeCode(request.TaxArticle),
                PayrollTaxInsuranceRules.NormalizeCode(request.RecipientTaxCategory),
                PayrollTaxInsuranceRules.NormalizeCode(request.CalculationMode),
                request.TaxMethod.Trim(),
                request.IsTaxResident,
                NormalizeNullableText(request.ExemptionReason),
                NormalizeNullableText(request.DecisionBasis),
                effectiveManualAmount,
                request.TreatyRatePercent,
                NormalizeNullableText(request.TreatyBasis));
            if (configurationErrors.Count > 0)
                return (false, string.Join(" ", configurationErrors));

            var npwpNumber = NormalizeNullableText(request.NpwpNumber);
            if (npwpNumber != null &&
                await _dbContext.Set<PrlTax>().AnyAsync(
                    x => !x.IsDelete &&
                         x.NpwpNumber == npwpNumber &&
                         (!currentId.HasValue || x.Id != currentId.Value),
                    cancellationToken))
            {
                return (false, "Nomor NPWP sudah digunakan oleh profil lain.");
            }

            return (true, null);
        }

        private void ApplyRequest(PrlTax entity, CreateWfpTaxRequest request, decimal? effectiveManualAmount)
        {
            // Nilai sebelum diubah: perubahan skema, nominal, metode, atau dasar membatalkan persetujuan nominal manual sebelumnya.
            var previousMode = entity.CalculationMode;
            var previousAmount = entity.ApprovedManualAmount;
            var previousBasis = entity.DecisionBasis;
            var previousMethod = entity.TaxMethod;

            entity.NpwpNumber = NormalizeNullableText(request.NpwpNumber);
            entity.TaxStatus = NormalizeAllowedValue(AllowedTaxStatuses, request.TaxStatus);
            entity.TaxMethod = NormalizeAllowedValue(AllowedTaxMethods, request.TaxMethod);
            entity.TaxCountryCode = request.TaxCountryCode.Trim().ToUpperInvariant();
            entity.TaxOfficeCode = NormalizeNullableText(request.TaxOfficeCode);
            entity.IsNpwpRegistered = request.IsNpwpRegistered;
            entity.IsTaxResident = request.IsTaxResident;
            entity.HasPreviousEmployer = request.HasPreviousEmployer;
            // Pajak ditanggung pemberi kerja adalah akibat metode Net, bukan isian terpisah: diturunkan Backend supaya keduanya tidak
            // dapat saling bertentangan.
            entity.IsEmployerBorneTax = string.Equals(entity.TaxMethod, TaxMethodKinds.Net, StringComparison.OrdinalIgnoreCase);
            entity.TaxArticle = PayrollTaxInsuranceRules.NormalizeCode(request.TaxArticle);
            entity.RecipientTaxCategory = PayrollTaxInsuranceRules.NormalizeCode(request.RecipientTaxCategory);
            entity.CalculationMode = PayrollTaxInsuranceRules.NormalizeCode(request.CalculationMode);
            entity.PolicyVersion = NormalizeNullableText(request.PolicyVersion);
            entity.ExemptionReason = entity.CalculationMode == TaxCalculationModes.ExemptOrNotApplicable
                ? NormalizeNullableText(request.ExemptionReason)
                : null;
            entity.DecisionBasis = NormalizeNullableText(request.DecisionBasis);
            entity.ApprovedManualAmount = entity.CalculationMode == TaxCalculationModes.ApprovedManualAmount
                ? effectiveManualAmount
                : null;
            // Tarif P3B hanya bermakna untuk PPh Pasal 26 dan hanya bersama dasarnya.
            entity.TreatyRatePercent = entity.TaxArticle == TaxArticles.Pph26 ? request.TreatyRatePercent : null;
            entity.TreatyBasis = entity.TreatyRatePercent.HasValue ? NormalizeNullableText(request.TreatyBasis) : null;

            // Persetujuan tersendiri atas nominal manual: nominal baru atau yang berubah (juga perubahan dasar/metode/skema) menunggu
            // persetujuan pengguna lain. Pemanggil tanpa ViewAmount mengirim ulang nominal yang sama sehingga tidak mereset persetujuan.
            if (entity.CalculationMode == TaxCalculationModes.ApprovedManualAmount)
            {
                var changed = previousMode != entity.CalculationMode
                    || previousAmount != entity.ApprovedManualAmount
                    || !string.Equals(previousBasis, entity.DecisionBasis, StringComparison.Ordinal)
                    || !string.Equals(previousMethod, entity.TaxMethod, StringComparison.OrdinalIgnoreCase)
                    || entity.ManualAmountApprovalStatus is null;
                if (changed)
                {
                    entity.ManualAmountApprovalStatus = ManualAmountApprovalStatuses.Pending;
                    entity.ManualAmountSubmittedAt = DateTime.UtcNow;
                    entity.ManualAmountSubmittedByUserId = GetCurrentUserId();
                    entity.ManualAmountDecidedAt = null;
                    entity.ManualAmountDecidedByUserId = null;
                    entity.ManualAmountDecisionNote = null;
                }
            }
            else
            {
                entity.ManualAmountApprovalStatus = null;
                entity.ManualAmountSubmittedAt = null;
                entity.ManualAmountSubmittedByUserId = null;
                entity.ManualAmountDecidedAt = null;
                entity.ManualAmountDecidedByUserId = null;
                entity.ManualAmountDecisionNote = null;
            }
            entity.PreviousEmployerTaxableIncome = request.PreviousEmployerTaxableIncome;
            entity.PreviousEmployerTaxPaid = request.PreviousEmployerTaxPaid;
            entity.AnnualNonTaxableIncome = request.AnnualNonTaxableIncome;
            entity.EffectiveStartDate = PayrollDateNormalizer.ToUtcDate(request.EffectiveStartDate);
            entity.EffectiveEndDate = PayrollDateNormalizer.ToUtcDate(request.EffectiveEndDate);
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
        }

        private async Task<bool> WorkforceProfileExistsAsync(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            return workforceProfileId != Guid.Empty &&
                   await _dbContext.MstWorkforceProfiles
                       .AsNoTracking()
                       .AnyAsync(
                           x => x.Id == workforceProfileId &&
                                x.IsActive &&
                                !x.IsDelete,
                           cancellationToken);
        }

        private string? GetUserDisplayName(Guid userId)
        {
            if (userId == Guid.Empty)
                return null;

            return _dbContext.Users
                .Where(x => x.Id == userId)
                .Select(x => x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode)
                .FirstOrDefault();
        }

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue("user_id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 25 : pageSize;
            pageSize = pageSize > 100 ? 100 : pageSize;
            return (pageNumber, pageSize);
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string NormalizeAllowedValue(
            IEnumerable<string> allowedValues,
            string value)
        {
            return allowedValues.First(x =>
                x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
