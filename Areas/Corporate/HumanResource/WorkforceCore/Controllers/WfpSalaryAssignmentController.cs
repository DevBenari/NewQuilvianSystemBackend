using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Options;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;
using System.Globalization;
using System.Security.Claims;

using SalaryAssignmentPagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs.WfpSalaryAssignmentResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers
{
    /// <summary>
    /// Penetapan gaji pegawai (HRD). PAY-BE-FIX-008: "disetujui" hanya bila ApprovedAt dan ApprovedByUserId terisi (IsActive tidak pernah
    /// berarti disetujui); perubahan hak finansial membatalkan persetujuan lama; penetapan yang sudah dipakai payroll yang disahkan tidak
    /// diubah isinya (hanya ditutup masa berlakunya lalu dibuat penetapan baru); dua penetapan utama aktif tidak boleh beririsan tanggal;
    /// penulisan berjalan di transaksi Serializable dengan advisory lock per pegawai.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/payroll-management/workforce-profiles/{workforceProfileId:guid}/salary-assignments")]
    // Alias kompatibilitas: rute di bawah ini adalah rute sebelum Payroll dipisah dari HRD; dihapus bertahap setelah seluruh consumer berpindah.
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/salary-assignments")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_CORE",
        moduleName: "Human Resource Workforce Core",
        displayName: "Workforce Salary Assignment",
        AreaName = "Corporate",
        ControllerName = "WfpSalaryAssignment",
        Description = "Corporate human resource workforce salary assignment",
        SortOrder = 10)]
    [Tags("Corporate / Payroll Management / Salary Assignment")]
    public class WfpSalaryAssignmentController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.WorkforceCore";
        private const string PrimaryIndexViolationMessage =
            "Database menolak dua penetapan gaji utama aktif untuk pegawai yang sama (indeks IX_WfpSalaryAssignment_WorkforceProfileId). "
            + "Muat ulang data pegawai ini lalu ulangi; bila berulang, periksa penetapan utama pegawai ini di HRD.";
        private static readonly string[] PaymentFrequencies = { "Monthly", "Weekly", "Biweekly", "Daily", "Hourly" };
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly AccessPermissionService _accessPermissionService;
        private readonly PayrollSalaryRangeRuleService _salaryRangeRuleService;
        private readonly PayrollPolicyOptions _policy;

        public WfpSalaryAssignmentController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            AccessPermissionService accessPermissionService,
            PayrollSalaryRangeRuleService salaryRangeRuleService,
            IOptions<PayrollPolicyOptions> policyOptions)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _accessPermissionService = accessPermissionService;
            _salaryRangeRuleService = salaryRangeRuleService;
            _policy = policyOptions.Value;
        }

        private Task<bool> CanViewAmountAsync() => _accessPermissionService.HasAccessAsync(User, "WfpSalaryAssignment", "ViewAmount");

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Workforce Salary Assignment", Description = "Melihat metadata salary assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpSalaryAssignment", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new WfpSalaryAssignmentFilterMetadataResponse
            {
                DefaultFilter = new WfpSalaryAssignmentDefaultFilterResponse(),
                CustomPeriods = BuildPeriods(),
                PaymentFrequencyOptions = PaymentFrequencies.Select(x => new WfpSalaryAssignmentStringOptionResponse { Value = x, Label = x }).ToList(),
                CurrencyOptions = new List<WfpSalaryAssignmentStringOptionResponse>
                {
                    new() { Value = "IDR", Label = "IDR" },
                    new() { Value = "USD", Label = "USD" },
                    new() { Value = "SGD", Label = "SGD" },
                    new() { Value = "EUR", Label = "EUR" }
                },
                SortOptions = new List<WfpSalaryAssignmentStringOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal efektif" },
                    new() { Value = "baseSalary", Label = "Gaji pokok" },
                    new() { Value = "paymentFrequency", Label = "Frekuensi pembayaran" },
                    new() { Value = "isPrimary", Label = "Primary" },
                    new() { Value = "approvedAt", Label = "Tanggal approval" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WfpSalaryAssignmentFilterMetadataResponse>.Ok(result, "Metadata filter salary assignment berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Workforce Salary Assignment", Description = "Melihat ringkasan salary assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpSalaryAssignment", "Read")]
        public async Task<IActionResult> GetSummary(Guid workforceProfileId, CancellationToken ct)
        {
            if (!await WorkforceExistsAsync(workforceProfileId, ct)) return WorkforceNotFound();

            var query = _dbContext.Set<WfpSalaryAssignment>().AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var approvalRequired = _policy.RequireApprovedSalaryAssignment;
            var today = DateTime.UtcNow.Date;
            var result = new WfpSalaryAssignmentSummaryResponse
            {
                TotalData = await query.CountAsync(ct),
                ActiveData = await query.CountAsync(x => x.IsActive, ct),
                InactiveData = await query.CountAsync(x => !x.IsActive, ct),
                PrimaryData = await query.CountAsync(x => x.IsPrimary && x.IsActive, ct),
                // Dua hitungan lama tetap berarti fakta persetujuan manual (kompatibilitas); jangan dibaca sebagai "perlu disetujui".
                ApprovedData = await query.CountAsync(WfpSalaryAssignmentRules.IsApprovedExpression, ct),
                PendingApprovalData = await query.CountAsync(WfpSalaryAssignmentRules.IsPendingExpression, ct),
                // Penambahan PAY-BE-FIX-010: kelayakan Payroll menurut kebijakan, supaya ringkasan tidak bertentangan dengan daftar.
                ApprovalRequired = approvalRequired,
                AutoByPolicyData = await query.CountAsync(
                    WfpSalaryAssignmentRules.AuthorizationStatusExpression(WfpSalaryAuthorizationStatuses.AutoByPolicy, approvalRequired), ct),
                AwaitingApprovalData = await query.CountAsync(
                    WfpSalaryAssignmentRules.AuthorizationStatusExpression(WfpSalaryAuthorizationStatuses.PendingApproval, approvalRequired), ct),
                PayrollUsableData = await query.CountAsync(WfpSalaryAssignmentRules.PayrollUsableExpression(approvalRequired, today), ct)
            };

            return Ok(ApiResponse<WfpSalaryAssignmentSummaryResponse>.Ok(result, "Ringkasan salary assignment berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Workforce Salary Assignment", Description = "Melihat salary assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpSalaryAssignment", "Read")]
        public async Task<IActionResult> GetAll(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? salaryStructureId,
            [FromQuery] Guid? salaryGradeId,
            [FromQuery] string? paymentFrequency,
            [FromQuery] string? currencyCode,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isApproved,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "effectiveStartDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken ct = default)
        {
            if (!await WorkforceExistsAsync(workforceProfileId, ct)) return WorkforceNotFound();
            NormalizePaging(ref pageNumber, ref pageSize);

            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);
            if (!dateRange.IsValid) return BadRequest(ApiResponse<object>.Fail(400, dateRange.ErrorMessage!));

            var query = BuildBaseQuery(workforceProfileId);
            if (dateRange.Start.HasValue) query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);
            if (dateRange.EndExclusive.HasValue) query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);
            if (salaryStructureId.HasValue && salaryStructureId.Value != Guid.Empty) query = query.Where(x => x.SalaryStructureId == salaryStructureId.Value);
            if (salaryGradeId.HasValue && salaryGradeId.Value != Guid.Empty) query = query.Where(x => x.SalaryGradeId == salaryGradeId.Value);
            if (!string.IsNullOrWhiteSpace(paymentFrequency)) query = query.Where(x => x.PaymentFrequency == NormalizeFrequency(paymentFrequency));
            if (!string.IsNullOrWhiteSpace(currencyCode)) query = query.Where(x => x.CurrencyCode == currencyCode.Trim().ToUpperInvariant());
            if (isPrimary.HasValue) query = query.Where(x => x.IsPrimary == isPrimary.Value);
            if (isApproved.HasValue)
                query = isApproved.Value
                    ? query.Where(WfpSalaryAssignmentRules.IsApprovedExpression)
                    : query.Where(WfpSalaryAssignmentRules.IsPendingExpression);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.CurrencyCode.ToLower().Contains(keyword) ||
                    x.PaymentFrequency.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.WorkforceProfile != null && (x.WorkforceProfile.ProfileCode.ToLower().Contains(keyword) || x.WorkforceProfile.DisplayName.ToLower().Contains(keyword))));
            }

            var totalData = await query.CountAsync(ct);
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            var actorNames = await GetActorNamesAsync(entities.SelectMany(x => new[] { x.CreateBy, x.UpdateBy, x.ApprovedByUserId ?? Guid.Empty }), ct);
            var includeAmounts = await CanViewAmountAsync();
            var usage = await LoadFinalPayrollUsageAsync(entities.Select(x => x.Id).ToList(), ct);
            var items = entities
                .Select(x => MapResponse(x, actorNames, includeAmounts, usage.TryGetValue(x.Id, out var lastFinalEnd) ? lastFinalEnd : (DateTime?)null, _policy.RequireApprovedSalaryAssignment))
                .ToList();

            return Ok(ApiResponse<SalaryAssignmentPagedResult>.Ok(new SalaryAssignmentPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data salary assignment berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Workforce Salary Assignment", Description = "Melihat detail salary assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpSalaryAssignment", "Read")]
        public async Task<IActionResult> GetById(Guid workforceProfileId, Guid id, CancellationToken ct)
        {
            var entity = await BuildBaseQuery(workforceProfileId).FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Salary assignment tidak ditemukan."));
            var actorNames = await GetActorNamesAsync(new[] { entity.CreateBy, entity.UpdateBy, entity.ApprovedByUserId ?? Guid.Empty }, ct);
            var includeAmounts = await CanViewAmountAsync();
            var lastFinalEnd = await GetLastFinalPayrollPeriodEndAsync(id, ct);
            return Ok(ApiResponse<WfpSalaryAssignmentResponse>.Ok(MapResponse(entity, actorNames, includeAmounts, lastFinalEnd, _policy.RequireApprovedSalaryAssignment), "Detail salary assignment berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Workforce Salary Assignment", Description = "Membuat salary assignment", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WfpSalaryAssignment", "Create")]
        public async Task<IActionResult> Create(Guid workforceProfileId, [FromBody] CreateWfpSalaryAssignmentRequest request, CancellationToken ct)
        {
            if (!await WorkforceExistsAsync(workforceProfileId, ct)) return WorkforceNotFound();
            var actor = GetCurrentUserId();

            try
            {
                return await PayrollTransactionRunner.RunAsync<IActionResult>(_dbContext, LockKey(workforceProfileId), async () =>
                {
                    var validation = await ValidateRequestAsync(workforceProfileId, null, request, ct);
                    if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

                    var now = DateTime.UtcNow;
                    var demoted = new List<Guid>();
                    if (request.IsPrimary && request.IsActive)
                    {
                        // Dua penetapan utama aktif tidak boleh beririsan tanggal: yang beririsan ditolak, tidak diam-diam diturunkan.
                        var overlap = await FindOverlappingPrimaryAsync(workforceProfileId, null, request.EffectiveStartDate, request.EffectiveEndDate, ct);
                        if (overlap != null) return ConflictReply(OverlapMessage(overlap));
                        demoted = await ClearPrimaryAsync(workforceProfileId, null, actor, now, ct);
                        if (demoted.Count > 0) await _dbContext.SaveChangesAsync(ct);
                    }

                    var entity = new WfpSalaryAssignment
                    {
                        Id = Guid.NewGuid(),
                        WorkforceProfileId = workforceProfileId,
                        SalaryStructureId = request.SalaryStructureId,
                        SalaryGradeId = request.SalaryGradeId,
                        EmployeeGradeId = NormalizeGuid(request.EmployeeGradeId),
                        PayrollPeriodId = NormalizeGuid(request.PayrollPeriodId),
                        PersonnelActionOrderId = NormalizeGuid(request.PersonnelActionOrderId),
                        // Gaji Dasar ditentukan server dari master Struktur Gaji (ValidateRequestAsync); nominal dari klien tidak pernah disimpan.
                        BaseSalary = validation.BaseSalary,
                        CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant(),
                        PaymentFrequency = NormalizeFrequency(request.PaymentFrequency),
                        EffectiveStartDate = request.EffectiveStartDate.Date,
                        EffectiveEndDate = request.EffectiveEndDate?.Date,
                        IsPrimary = request.IsPrimary,
                        IsConfidential = request.IsConfidential,
                        IsActive = request.IsActive,
                        // Create tidak pernah mengisi metadata persetujuan manual (tidak ada jalur yang membuatnya "langsung disetujui" oleh
                        // seseorang). Apakah penetapan baru lalu menunggu persetujuan atau sah otomatis ditentukan kebijakan
                        // Payroll:Policy:RequireApprovedSalaryAssignment saat dibaca (AuthorizationStatus), bukan oleh kolom ini (PAY-BE-FIX-010).
                        ApprovedAt = null,
                        ApprovedByUserId = null,
                        Description = Normalize(request.Description),
                        CreateDateTime = now,
                        CreateBy = actor,
                        IsDelete = false,
                        IsCancel = false
                    };

                    _dbContext.Set<WfpSalaryAssignment>().Add(entity);
                    await _dbContext.SaveChangesAsync(ct);
                    await AuditAsync("WfpSalaryAssignment.Create", "Salary assignment berhasil dibuat.", entity, actor,
                        new { DemotedPrimaryAssignmentIds = demoted });
                    return Ok(ApiResponse<object>.Ok(new { entity.Id, DemotedPrimaryAssignmentIds = demoted },
                        _policy.RequireApprovedSalaryAssignment
                            ? "Salary assignment berhasil dibuat. Penetapan ini menunggu persetujuan dan belum dipakai payroll."
                            : "Salary assignment berhasil dibuat."));
                }, ct);
            }
            catch (DbUpdateException ex) when (IsPrimaryIndexViolation(ex))
            {
                return ConflictReply(PrimaryIndexViolationMessage);
            }
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Workforce Salary Assignment", Description = "Mengubah salary assignment", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpSalaryAssignment", "Update")]
        public async Task<IActionResult> Update(Guid workforceProfileId, Guid id, [FromBody] UpdateWfpSalaryAssignmentRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();

            try
            {
                return await PayrollTransactionRunner.RunAsync<IActionResult>(_dbContext, LockKey(workforceProfileId), async () =>
                {
                    var entity = await FindEntityAsync(workforceProfileId, id, ct);
                    if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Salary assignment tidak ditemukan."));
                    // Entity (terlacak, belum diubah) diteruskan supaya nominal tersimpan dipertahankan bila struktur/grade tidak berubah.
                    var validation = await ValidateRequestAsync(workforceProfileId, entity, request, ct);
                    if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

                    var now = DateTime.UtcNow;
                    var start = request.EffectiveStartDate.Date;
                    DateTime? end = request.EffectiveEndDate?.Date;
                    var currency = request.CurrencyCode.Trim().ToUpperInvariant();
                    var frequency = NormalizeFrequency(request.PaymentFrequency);

                    // Perubahan finansial dinilai terhadap nominal hasil penentuan server, bukan nominal kiriman klien.
                    var financialChange = WfpSalaryAssignmentRules.HasFinancialChange(
                        entity, request.SalaryStructureId, request.SalaryGradeId, validation.BaseSalary, currency, frequency, start, end);
                    var closureOnly = WfpSalaryAssignmentRules.IsClosureOnly(
                        entity, request.SalaryStructureId, request.SalaryGradeId, validation.BaseSalary, currency, frequency, start, end);
                    var wasApproved = WfpSalaryAssignmentRules.IsApproved(entity.ApprovedAt, entity.ApprovedByUserId);

                    // Penetapan yang sudah dipakai payroll yang disahkan tidak boleh ditulis ulang: koreksinya adalah menutup masa berlaku lalu
                    // membuat penetapan baru yang sah. Snapshot run yang disahkan tidak pernah tersentuh.
                    var lastFinalEnd = await GetLastFinalPayrollPeriodEndAsync(id, ct);
                    if (lastFinalEnd.HasValue)
                    {
                        if (financialChange && !closureOnly)
                            return ConflictReply(
                                "Penetapan ini sudah dipakai payroll yang disahkan, jadi struktur, grade, gaji pokok, mata uang, frekuensi, dan tanggal mulainya tidak dapat diubah. "
                                + "Tutup masa berlakunya (isi tanggal akhir) lalu buat penetapan gaji baru.");
                        if (closureOnly && end!.Value < lastFinalEnd.Value.Date)
                            return ConflictReply(
                                $"Masa berlaku tidak boleh ditutup sebelum akhir periode payroll terakhir yang sudah disahkan ({Fmt(lastFinalEnd.Value)}).");
                        if (entity.IsActive && !request.IsActive)
                            return ConflictReply("Penetapan yang sudah dipakai payroll yang disahkan tidak dapat dinonaktifkan. Tutup masa berlakunya.");
                    }

                    var demoted = new List<Guid>();
                    if (request.IsPrimary && request.IsActive)
                    {
                        var overlap = await FindOverlappingPrimaryAsync(workforceProfileId, id, start, end, ct);
                        if (overlap != null) return ConflictReply(OverlapMessage(overlap));
                        demoted = await ClearPrimaryAsync(workforceProfileId, id, actor, now, ct);
                        if (demoted.Count > 0) await _dbContext.SaveChangesAsync(ct);
                    }

                    // Mengubah hak finansial membatalkan persetujuan lama dan butuh persetujuan baru; penutupan masa berlaku tidak.
                    var approvalReset = financialChange && !closureOnly && wasApproved;
                    if (approvalReset)
                    {
                        entity.ApprovedAt = null;
                        entity.ApprovedByUserId = null;
                    }

                    entity.SalaryStructureId = request.SalaryStructureId;
                    entity.SalaryGradeId = request.SalaryGradeId;
                    entity.EmployeeGradeId = NormalizeGuid(request.EmployeeGradeId);
                    if (request.PayrollPeriodId.HasValue && request.PayrollPeriodId.Value != Guid.Empty)
                    {
                        entity.PayrollPeriodId = request.PayrollPeriodId.Value;
                    }
                    // Nilai historis PayrollPeriodId dan PersonnelActionOrderId dipertahankan jika request tidak menyertakannya.
                    if (request.PersonnelActionOrderId.HasValue && request.PersonnelActionOrderId.Value != Guid.Empty)
                    {
                        entity.PersonnelActionOrderId = request.PersonnelActionOrderId.Value;
                    }
                    entity.BaseSalary = validation.BaseSalary;
                    entity.CurrencyCode = currency;
                    entity.PaymentFrequency = frequency;
                    entity.EffectiveStartDate = start;
                    entity.EffectiveEndDate = end;
                    entity.IsPrimary = request.IsPrimary;
                    entity.IsConfidential = request.IsConfidential;
                    entity.IsActive = request.IsActive;
                    entity.Description = Normalize(request.Description);
                    entity.UpdateDateTime = now;
                    entity.UpdateBy = actor;
                    await _dbContext.SaveChangesAsync(ct);
                    await AuditAsync("WfpSalaryAssignment.Update", "Salary assignment diperbarui.", entity, actor,
                        new { FinancialChange = financialChange, ClosureOnly = closureOnly, ApprovalReset = approvalReset, DemotedPrimaryAssignmentIds = demoted });

                    var message = approvalReset
                        ? _policy.RequireApprovedSalaryAssignment
                            ? "Salary assignment berhasil diperbarui. Karena data gaji berubah, persetujuan sebelumnya dibatalkan dan penetapan perlu disetujui ulang sebelum dipakai payroll."
                            : "Salary assignment berhasil diperbarui. Karena data gaji berubah, persetujuan sebelumnya dibatalkan."
                        : "Salary assignment berhasil diperbarui.";
                    return Ok(ApiResponse<object>.Ok(new
                    {
                        ApprovalReset = approvalReset,
                        ApprovalStatus = WfpSalaryAssignmentRules.StatusOf(entity.ApprovedAt, entity.ApprovedByUserId),
                        DemotedPrimaryAssignmentIds = demoted
                    }, message));
                }, ct);
            }
            catch (DbUpdateException ex) when (IsPrimaryIndexViolation(ex))
            {
                return ConflictReply(PrimaryIndexViolationMessage);
            }
        }

        [HttpPatch("{id:guid}/approval")]
        [AccessAction("Update", "Approve Workforce Salary Assignment", Description = "Melakukan approval salary assignment", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpSalaryAssignment", "Update")]
        public async Task<IActionResult> Approve(Guid workforceProfileId, Guid id, [FromBody] ApproveWfpSalaryAssignmentRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (request.IsApproved && actor == Guid.Empty)
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(403, "Identitas pengguna tidak dapat ditentukan, jadi persetujuan tidak dapat dicatat."));

            return await PayrollTransactionRunner.RunAsync<IActionResult>(_dbContext, LockKey(workforceProfileId), async () =>
            {
                var entity = await FindEntityAsync(workforceProfileId, id, ct);
                if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Salary assignment tidak ditemukan."));
                var wasApproved = WfpSalaryAssignmentRules.IsApproved(entity.ApprovedAt, entity.ApprovedByUserId);
                var now = DateTime.UtcNow;

                if (request.IsApproved)
                {
                    // Idempoten: penetapan yang sudah disetujui tidak ditimpa pengesahnya.
                    if (wasApproved) return Ok(ApiResponse<object>.Ok(null, "Salary assignment sudah disetujui."));
                    if (!entity.IsActive)
                        return ConflictReply("Salary assignment yang tidak aktif tidak dapat disetujui. Aktifkan lebih dulu.");

                    // NEEDS CONFIRMATION (matriks maker-checker kompensasi): pemisahan pembuat dan pengesah baru berlaku bila kebijakan
                    // Payroll:Policy:RequireDifferentApprover dinyalakan pemilik proses (nilai bawaan false tidak diubah oleh kode).
                    // Skema belum menyimpan pengaju terpisah; pembuat/pengubah terakhir dipakai sebagai pengaju.
                    var maker = entity.UpdateDateTime.HasValue && entity.UpdateBy != Guid.Empty ? entity.UpdateBy : entity.CreateBy;
                    if (_policy.RequireDifferentApprover && maker != Guid.Empty && maker == actor)
                        return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(403,
                            "Pembuat atau pengubah terakhir penetapan gaji tidak boleh menyetujui penetapan yang sama (kebijakan pemisahan kewenangan)."));

                    var rangeCheck = await _salaryRangeRuleService.ValidateSalaryAssignmentAsync(entity.SalaryStructureId, entity.SalaryGradeId, entity.BaseSalary, entity.CurrencyCode, ct);
                    if (!rangeCheck.IsValid) return BadRequest(ApiResponse<object>.Fail(400, rangeCheck.Message!));

                    if (entity.IsPrimary)
                    {
                        var overlap = await FindOverlappingPrimaryAsync(workforceProfileId, id, entity.EffectiveStartDate, entity.EffectiveEndDate, ct);
                        if (overlap != null) return ConflictReply(OverlapMessage(overlap));
                    }

                    entity.ApprovedByUserId = actor;
                    entity.ApprovedAt = now;
                }
                else
                {
                    if (!wasApproved) return Ok(ApiResponse<object>.Ok(null, "Salary assignment memang belum disetujui."));
                    if (await GetLastFinalPayrollPeriodEndAsync(id, ct) != null)
                        return ConflictReply("Persetujuan penetapan yang sudah dipakai payroll yang disahkan tidak dapat dibatalkan. Tutup masa berlakunya.");

                    entity.ApprovedByUserId = null;
                    entity.ApprovedAt = null;
                }

                entity.UpdateDateTime = now;
                entity.UpdateBy = actor;
                await _dbContext.SaveChangesAsync(ct);
                await AuditAsync(request.IsApproved ? "WfpSalaryAssignment.Approve" : "WfpSalaryAssignment.Unapprove",
                    request.IsApproved ? "Salary assignment disetujui." : "Approval salary assignment dibatalkan.", entity, actor);
                return Ok(ApiResponse<object>.Ok(null, request.IsApproved ? "Salary assignment berhasil disetujui." : "Approval salary assignment berhasil dibatalkan."));
            }, ct);
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Workforce Salary Assignment", Description = "Mengubah status salary assignment", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpSalaryAssignment", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid workforceProfileId, Guid id, [FromBody] UpdateWfpSalaryAssignmentStatusRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();

            try
            {
                return await PayrollTransactionRunner.RunAsync<IActionResult>(_dbContext, LockKey(workforceProfileId), async () =>
                {
                    var entity = await FindEntityAsync(workforceProfileId, id, ct);
                    if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Salary assignment tidak ditemukan."));
                    var now = DateTime.UtcNow;
                    var demoted = new List<Guid>();

                    if (request.IsActive)
                    {
                        var rangeCheck = await _salaryRangeRuleService.ValidateSalaryAssignmentAsync(entity.SalaryStructureId, entity.SalaryGradeId, entity.BaseSalary, entity.CurrencyCode, ct);
                        if (!rangeCheck.IsValid) return BadRequest(ApiResponse<object>.Fail(400, rangeCheck.Message!));

                        if (entity.IsPrimary && !entity.IsActive)
                        {
                            var overlap = await FindOverlappingPrimaryAsync(workforceProfileId, id, entity.EffectiveStartDate, entity.EffectiveEndDate, ct);
                            if (overlap != null) return ConflictReply(OverlapMessage(overlap));
                            demoted = await ClearPrimaryAsync(workforceProfileId, id, actor, now, ct);
                            if (demoted.Count > 0) await _dbContext.SaveChangesAsync(ct);
                        }
                    }
                    else if (entity.IsActive && await GetLastFinalPayrollPeriodEndAsync(id, ct) != null)
                    {
                        return ConflictReply("Penetapan yang sudah dipakai payroll yang disahkan tidak dapat dinonaktifkan. Tutup masa berlakunya.");
                    }

                    entity.IsActive = request.IsActive;
                    if (!request.IsActive) entity.IsPrimary = false;
                    entity.UpdateDateTime = now;
                    entity.UpdateBy = actor;
                    await _dbContext.SaveChangesAsync(ct);
                    await AuditAsync("WfpSalaryAssignment.UpdateStatus", "Status salary assignment diperbarui.", entity, actor,
                        new { DemotedPrimaryAssignmentIds = demoted });
                    return Ok(ApiResponse<object>.Ok(null, "Status salary assignment berhasil diperbarui."));
                }, ct);
            }
            catch (DbUpdateException ex) when (IsPrimaryIndexViolation(ex))
            {
                return ConflictReply(PrimaryIndexViolationMessage);
            }
        }

        [HttpPatch("{id:guid}/primary")]
        [AccessAction("Update", "Update Workforce Salary Assignment", Description = "Mengatur primary salary assignment", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpSalaryAssignment", "Update")]
        public async Task<IActionResult> SetPrimary(Guid workforceProfileId, Guid id, [FromBody] SetWfpSalaryAssignmentPrimaryRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();

            try
            {
                return await PayrollTransactionRunner.RunAsync<IActionResult>(_dbContext, LockKey(workforceProfileId), async () =>
                {
                    var entity = await FindEntityAsync(workforceProfileId, id, ct);
                    if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Salary assignment tidak ditemukan."));
                    if (request.IsPrimary && !entity.IsActive) return BadRequest(ApiResponse<object>.Fail(400, "Salary assignment tidak aktif tidak dapat dijadikan primary."));

                    var now = DateTime.UtcNow;
                    var demoted = new List<Guid>();
                    if (request.IsPrimary)
                    {
                        var rangeCheck = await _salaryRangeRuleService.ValidateSalaryAssignmentAsync(entity.SalaryStructureId, entity.SalaryGradeId, entity.BaseSalary, entity.CurrencyCode, ct);
                        if (!rangeCheck.IsValid) return BadRequest(ApiResponse<object>.Fail(400, rangeCheck.Message!));

                        // Penetapan utama lain yang beririsan tanggal ditolak (tidak diam-diam diturunkan). Yang tidak beririsan diturunkan
                        // karena indeks unik database saat ini hanya mengizinkan satu penetapan utama aktif per pegawai (lihat laporan PAY-BE-FIX-008).
                        var overlap = await FindOverlappingPrimaryAsync(workforceProfileId, id, entity.EffectiveStartDate, entity.EffectiveEndDate, ct);
                        if (overlap != null) return ConflictReply(OverlapMessage(overlap));
                        demoted = await ClearPrimaryAsync(workforceProfileId, id, actor, now, ct);
                        if (demoted.Count > 0) await _dbContext.SaveChangesAsync(ct);
                    }

                    entity.IsPrimary = request.IsPrimary;
                    entity.UpdateDateTime = now;
                    entity.UpdateBy = actor;
                    await _dbContext.SaveChangesAsync(ct);
                    await AuditAsync("WfpSalaryAssignment.SetPrimary", "Primary salary assignment diperbarui.", entity, actor,
                        new { DemotedPrimaryAssignmentIds = demoted });
                    return Ok(ApiResponse<object>.Ok(new { DemotedPrimaryAssignmentIds = demoted }, "Primary salary assignment berhasil diperbarui."));
                }, ct);
            }
            catch (DbUpdateException ex) when (IsPrimaryIndexViolation(ex))
            {
                return ConflictReply(PrimaryIndexViolationMessage);
            }
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Workforce Salary Assignment", Description = "Menghapus salary assignment", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WfpSalaryAssignment", "Delete")]
        public async Task<IActionResult> Delete(Guid workforceProfileId, Guid id, CancellationToken ct)
        {
            var actor = GetCurrentUserId();

            return await PayrollTransactionRunner.RunAsync<IActionResult>(_dbContext, LockKey(workforceProfileId), async () =>
            {
                var entity = await FindEntityAsync(workforceProfileId, id, ct);
                if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Salary assignment tidak ditemukan."));
                if (await GetLastFinalPayrollPeriodEndAsync(id, ct) != null)
                    return ConflictReply("Penetapan yang sudah dipakai payroll yang disahkan tidak dapat dihapus. Tutup masa berlakunya agar riwayat tetap utuh.");

                var now = DateTime.UtcNow;
                entity.IsDelete = true;
                entity.IsActive = false;
                entity.IsPrimary = false;
                entity.DeleteDateTime = now;
                entity.DeleteBy = actor;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actor;
                await _dbContext.SaveChangesAsync(ct);
                await AuditAsync("WfpSalaryAssignment.Delete", "Salary assignment dihapus (ditandai).", entity, actor);
                return Ok(ApiResponse<object>.Ok(null, "Salary assignment berhasil dihapus."));
            }, ct);
        }

        private IQueryable<WfpSalaryAssignment> BuildBaseQuery(Guid workforceProfileId) =>
            _dbContext.Set<WfpSalaryAssignment>().AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.SalaryStructure)
                .Include(x => x.SalaryGrade)
                .Include(x => x.EmployeeGrade)
                .Include(x => x.PayrollPeriod)
                .Include(x => x.PersonnelActionOrder)
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

        private static IOrderedQueryable<WfpSalaryAssignment> ApplySorting(IQueryable<WfpSalaryAssignment> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "effectiveStartDate").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "basesalary" => desc ? query.OrderByDescending(x => x.BaseSalary) : query.OrderBy(x => x.BaseSalary),
                "paymentfrequency" => desc ? query.OrderByDescending(x => x.PaymentFrequency) : query.OrderBy(x => x.PaymentFrequency),
                "isprimary" => desc ? query.OrderByDescending(x => x.IsPrimary).ThenByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.IsPrimary).ThenByDescending(x => x.EffectiveStartDate),
                "approvedat" => desc ? query.OrderByDescending(x => x.ApprovedAt) : query.OrderBy(x => x.ApprovedAt),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate),
                _ => desc ? query.OrderByDescending(x => x.EffectiveStartDate).ThenByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.EffectiveStartDate).ThenBy(x => x.CreateDateTime)
            };
        }

        private const string DerivedAmountMismatchMessage =
            "Gaji Dasar ditentukan otomatis dari Struktur Gaji yang dipilih dan tidak dapat diisi atau diubah manual. "
            + "Nominal pada permintaan tidak sama dengan Gaji Dasar struktur. Muat ulang pilihan Struktur Gaji lalu simpan kembali tanpa mengubah nominal.";
        private const string StoredAmountMismatchMessage =
            "Gaji Dasar penetapan yang sudah tersimpan tidak dapat diubah dari formulir ini. "
            + "Untuk mengubah nominal, tutup masa berlaku penetapan ini lalu buat penetapan baru dengan Struktur Gaji yang sesuai.";

        private static (bool IsValid, string? ErrorMessage, decimal BaseSalary) Reject(string message) => (false, message, 0m);

        /// <summary>
        /// Validasi permintaan sekaligus MENENTUKAN Gaji Dasar di server (PAY-BE-FIX-011). Nominal dari klien tidak pernah otoritatif:
        /// penetapan baru atau pergantian struktur/grade memakai MstSalaryStructure.DefaultBaseSalary; bila struktur dan grade tidak berubah,
        /// nominal tersimpan dipertahankan (histori tidak ditimpa perubahan master). <paramref name="existing"/> null berarti Create.
        /// </summary>
        private async Task<(bool IsValid, string? ErrorMessage, decimal BaseSalary)> ValidateRequestAsync(
            Guid workforceProfileId, WfpSalaryAssignment? existing, CreateWfpSalaryAssignmentRequest request, CancellationToken ct)
        {
            var excludeId = existing?.Id;
            if (request.SalaryStructureId == Guid.Empty) return Reject("Salary structure wajib dipilih.");
            if (request.SalaryGradeId == Guid.Empty) return Reject("Salary grade wajib dipilih.");
            if (request.BaseSalary.HasValue && request.BaseSalary.Value < 0) return Reject("Base salary tidak boleh negatif.");
            if (string.IsNullOrWhiteSpace(request.CurrencyCode) || request.CurrencyCode.Trim().Length != 3) return Reject("Currency code wajib terdiri dari 3 karakter.");
            if (!PaymentFrequencies.Contains(NormalizeFrequency(request.PaymentFrequency), StringComparer.OrdinalIgnoreCase)) return Reject("Payment frequency tidak valid.");
            if (request.EffectiveStartDate == default) return Reject("Effective start date wajib diisi.");
            if (request.EffectiveEndDate.HasValue && request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Date) return Reject("Effective end date tidak boleh lebih kecil dari effective start date.");
            if (!await ExistsActiveIfProvidedAsync<MstEmployeeGrade>(request.EmployeeGradeId, ct)) return Reject("Employee grade tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstPayrollPeriod>(request.PayrollPeriodId, ct)) return Reject("Payroll period tidak ditemukan atau tidak aktif.");

            // SK yang ditautkan harus milik pegawai yang sama; SK hanya referensi dan tidak pernah mengubah nominal gaji.
            if (request.PersonnelActionOrderId.HasValue && request.PersonnelActionOrderId.Value != Guid.Empty)
            {
                var orderId = request.PersonnelActionOrderId.Value;
                var orderOk = await _dbContext.Set<HrdPersonnelActionOrder>().AsNoTracking()
                    .AnyAsync(o => o.Id == orderId && o.WorkforceProfileId == workforceProfileId && !o.IsDelete, ct);
                if (!orderOk) return Reject("SK Kepegawaian yang ditautkan tidak ditemukan atau bukan milik pegawai ini.");
            }

            // Penentuan Gaji Dasar. Struktur/grade baru (atau pergantian) memakai DefaultBaseSalary master; tanggal mulai yang berubah pada
            // struktur yang sama hanya memeriksa keabsahan struktur pada tanggal itu. Selain itu nominal tersimpan tidak disentuh.
            var structureChanged = existing == null
                || existing.SalaryStructureId != request.SalaryStructureId
                || existing.SalaryGradeId != request.SalaryGradeId;
            var startChanged = existing != null && existing.EffectiveStartDate.Date != request.EffectiveStartDate.Date;

            decimal baseSalary;
            if (structureChanged || startChanged)
            {
                var basis = await _salaryRangeRuleService.ResolveSalaryAssignmentBasisAsync(
                    request.SalaryStructureId, request.SalaryGradeId, request.EffectiveStartDate, requireDefaultBaseSalary: structureChanged, ct);
                if (!basis.IsValid) return Reject(basis.Message!);
                baseSalary = structureChanged ? basis.DefaultBaseSalary : existing!.BaseSalary;
            }
            else
            {
                baseSalary = existing!.BaseSalary;
            }

            // Klien lama yang masih mengirim nominal: hanya diterima bila identik dengan hasil penentuan server; selain itu ditolak, bukan disimpan.
            if (request.BaseSalary.HasValue && Math.Round(request.BaseSalary.Value, 2) != Math.Round(baseSalary, 2))
                return Reject(structureChanged ? DerivedAmountMismatchMessage : StoredAmountMismatchMessage);

            var rangeCheck = await _salaryRangeRuleService.ValidateSalaryAssignmentAsync(request.SalaryStructureId, request.SalaryGradeId, baseSalary, request.CurrencyCode, ct);
            if (!rangeCheck.IsValid) return Reject(rangeCheck.Message!);

            // Pemeriksaan lama dipertahankan: struktur dan grade yang sama tidak boleh beririsan (apa pun status utama/aktifnya).
            var overlaps = await _dbContext.Set<WfpSalaryAssignment>().AsNoTracking().AnyAsync(x =>
                x.WorkforceProfileId == workforceProfileId && x.Id != excludeId && !x.IsDelete &&
                x.EffectiveStartDate <= (request.EffectiveEndDate ?? DateTime.MaxValue).Date &&
                (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= request.EffectiveStartDate.Date) &&
                x.SalaryStructureId == request.SalaryStructureId && x.SalaryGradeId == request.SalaryGradeId, ct);
            if (overlaps) return Reject("Periode salary assignment dengan struktur dan grade yang sama beririsan.");
            return (true, null, baseSalary);
        }

        /// <summary>
        /// Penetapan utama aktif lain milik pegawai ini yang masa berlakunya beririsan (apa pun status persetujuannya); null bila tidak ada.
        /// Berlaku untuk SEMUA penetapan utama, bukan hanya kombinasi struktur dan grade yang sama (P1-01).
        /// </summary>
        private async Task<WfpSalaryAssignment?> FindOverlappingPrimaryAsync(
            Guid workforceProfileId, Guid? excludeId, DateTime start, DateTime? end, CancellationToken ct)
        {
            var startDate = start.Date;
            var endBound = (end ?? DateTime.MaxValue).Date;
            return await _dbContext.Set<WfpSalaryAssignment>().AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete && x.IsActive && x.IsPrimary
                    && (excludeId == null || x.Id != excludeId)
                    && x.EffectiveStartDate <= endBound
                    && (x.EffectiveEndDate == null || x.EffectiveEndDate >= startDate))
                .OrderBy(x => x.EffectiveStartDate)
                .FirstOrDefaultAsync(ct);
        }

        private static string OverlapMessage(WfpSalaryAssignment other)
        {
            var status = WfpSalaryAssignmentRules.IsApproved(other.ApprovedAt, other.ApprovedByUserId) ? "disetujui" : "menunggu persetujuan";
            var range = Fmt(other.EffectiveStartDate) + " s.d. " + (other.EffectiveEndDate.HasValue ? Fmt(other.EffectiveEndDate.Value) : "tanpa batas akhir");
            return $"Sudah ada penetapan gaji utama aktif ({status}) untuk {range} yang beririsan dengan masa berlaku ini. "
                + "Tutup masa berlakunya lebih dulu (isi tanggal akhir sebelum tanggal mulai yang baru) atau nonaktifkan, supaya tidak ada dua penetapan utama pada tanggal yang sama.";
        }

        /// <summary>
        /// Menurunkan penetapan utama aktif lain dari Utama. Dipanggil HANYA setelah <see cref="FindOverlappingPrimaryAsync"/> memastikan tidak ada yang
        /// beririsan, jadi yang diturunkan adalah penetapan utama yang tidak beririsan tanggal (riwayat atau yang akan datang). Penurunan ini
        /// dipaksa indeks unik IX_WfpSalaryAssignment_WorkforceProfileId (satu penetapan utama aktif per pegawai); pelonggarannya menunggu
        /// perubahan skema yang dipisahkan sebagai dependency. Id yang diturunkan dikembalikan dan dicatat di audit supaya tidak diam-diam.
        /// </summary>
        private async Task<List<Guid>> ClearPrimaryAsync(Guid workforceProfileId, Guid? excludeId, Guid actor, DateTime now, CancellationToken ct)
        {
            var rows = await _dbContext.Set<WfpSalaryAssignment>().Where(x => x.WorkforceProfileId == workforceProfileId && x.IsPrimary && x.IsActive && !x.IsDelete && x.Id != excludeId).ToListAsync(ct);
            foreach (var row in rows) { row.IsPrimary = false; row.UpdateDateTime = now; row.UpdateBy = actor; }
            return rows.Select(x => x.Id).ToList();
        }

        /// <summary>Akhir periode payroll terakhir yang DISAHKAN dan memakai penetapan ini; null bila belum pernah dipakai payroll yang disahkan.</summary>
        private async Task<DateTime?> GetLastFinalPayrollPeriodEndAsync(Guid assignmentId, CancellationToken ct)
        {
            var usage = await LoadFinalPayrollUsageAsync(new List<Guid> { assignmentId }, ct);
            return usage.TryGetValue(assignmentId, out var end) ? end : null;
        }

        private async Task<Dictionary<Guid, DateTime>> LoadFinalPayrollUsageAsync(IReadOnlyCollection<Guid> assignmentIds, CancellationToken ct)
        {
            if (assignmentIds.Count == 0) return new Dictionary<Guid, DateTime>();
            var rows = await _dbContext.PrlPayrollRunEmployees.AsNoTracking()
                .Where(e => !e.IsDelete && e.IsFinalized && e.SalaryAssignmentId != null && assignmentIds.Contains(e.SalaryAssignmentId.Value)
                    && e.PayrollRun != null && !e.PayrollRun.IsDelete)
                .Select(e => new { Id = e.SalaryAssignmentId!.Value, End = e.PayrollRun!.PeriodEndDateSnapshot })
                .ToListAsync(ct);
            return rows.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.Max(x => x.End));
        }

        private async Task<bool> WorkforceExistsAsync(Guid id, CancellationToken ct) =>
            await _dbContext.Set<MstWorkforceProfile>().AsNoTracking().AnyAsync(x => x.Id == id && x.IsActive && !x.IsDelete, ct);

        private IActionResult WorkforceNotFound() => NotFound(ApiResponse<object>.Fail(404, "Workforce profile tidak ditemukan atau sudah tidak aktif."));

        private async Task<WfpSalaryAssignment?> FindEntityAsync(Guid workforceProfileId, Guid id, CancellationToken ct) =>
            await _dbContext.Set<WfpSalaryAssignment>().FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, ct);

        private Task AuditAsync(string action, string message, WfpSalaryAssignment entity, Guid actor, object? extra = null) =>
            // Tanpa gaji pokok: penetapan bersifat rahasia; hanya identitas, status, aktor, dan keterangan perubahan.
            _loggerService.AuditAsync(LogCategory, action, message, new
            {
                EntityId = entity.Id,
                entity.WorkforceProfileId,
                entity.SalaryStructureId,
                entity.SalaryGradeId,
                entity.IsPrimary,
                entity.IsActive,
                ApprovalStatus = WfpSalaryAssignmentRules.StatusOf(entity.ApprovedAt, entity.ApprovedByUserId),
                ActorUserId = actor,
                Detail = extra
            });

        private static string LockKey(Guid workforceProfileId) => $"WFP_SALARY_ASSIGNMENT_{workforceProfileId:N}";

        private IActionResult ConflictReply(string message) => Conflict(ApiResponse<object>.Fail(409, message));

        private static bool IsPrimaryIndexViolation(DbUpdateException ex) =>
            ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

        private static string Fmt(DateTime value) => value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

        private static WfpSalaryAssignmentResponse MapResponse(
            WfpSalaryAssignment x, IReadOnlyDictionary<Guid, string?> actorNames, bool includeAmounts = true,
            DateTime? lastFinalPayrollPeriodEnd = null, bool approvalRequired = false)
        {
            var today = DateTime.UtcNow.Date;
            var gradeMin = x.SalaryGrade?.MinimumSalary ?? 0m;
            var gradeMax = x.SalaryGrade?.MaximumSalary ?? 0m;
            var structMin = x.SalaryStructure?.MinimumBaseSalary;
            var structMax = x.SalaryStructure?.MaximumBaseSalary;

            var range = PayrollSalaryRangeRuleService.CalculateEffectiveRange(gradeMin, gradeMax, structMin, structMax);
            var approved = WfpSalaryAssignmentRules.IsApproved(x.ApprovedAt, x.ApprovedByUserId);

            // PAY-BE-FIX-011: keabsahan master hari ini dan perbandingan nominal tersimpan dengan Gaji Dasar bawaan struktur saat ini.
            // Hanya informasi untuk ditinjau; penetapan lama tidak pernah diubah otomatis mengikuti master.
            var validity = PayrollSalaryRangeRuleService.AssessMasterValidity(
                x.SalaryStructure?.IsActive, x.SalaryStructure?.EffectiveStartDate, x.SalaryStructure?.EffectiveEndDate, x.SalaryGrade?.IsActive, today);
            bool? differsFromStructureDefault = includeAmounts && x.SalaryStructure != null
                ? Math.Round(x.BaseSalary, 2) != Math.Round(x.SalaryStructure.DefaultBaseSalary, 2)
                : null;

            // Satu aturan untuk teks lama dan kode baru (PAY-BE-FIX-010). Teks OperationalStatus dipertahankan persis seperti sebelumnya:
            // nonaktif, lalu "Menunggu Persetujuan" (hanya bila kebijakan mensyaratkan approval dan belum tercatat), lalu menurut tanggal.
            var standing = WfpSalaryAssignmentRules.StandingOf(
                x.IsActive, x.IsPrimary, x.EffectiveStartDate, x.EffectiveEndDate, x.ApprovedAt, x.ApprovedByUserId, approvalRequired, today);

            string statusText;
            if (standing.OperationalStatus == WfpSalaryOperationalStatuses.Inactive)
            {
                statusText = "Nonaktif";
            }
            else if (standing.AuthorizationStatus == WfpSalaryAuthorizationStatuses.PendingApproval)
            {
                // Payroll mensyaratkan approval (RequireApprovedSalaryAssignment menyala): aktif tetapi belum disetujui bukan "Berlaku",
                // ia belum dipakai payroll. Bila approval tidak disyaratkan (bawaan), status operasional mengikuti tanggal seperti semula.
                statusText = "Menunggu Persetujuan";
            }
            else if (standing.OperationalStatus == WfpSalaryOperationalStatuses.Scheduled)
            {
                statusText = "Terjadwal";
            }
            else if (standing.OperationalStatus == WfpSalaryOperationalStatuses.Expired)
            {
                statusText = "Berakhir";
            }
            else
            {
                statusText = "Berlaku";
            }

            if (x.IsPrimary)
            {
                statusText = $"{statusText} · Utama";
            }

            return new()
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
                SalaryStructureId = x.SalaryStructureId,
                SalaryStructureCode = x.SalaryStructure?.SalaryStructureCode,
                SalaryStructureName = x.SalaryStructure?.SalaryStructureName,
                SalaryGradeId = x.SalaryGradeId,
                SalaryGradeCode = x.SalaryGrade?.SalaryGradeCode,
                SalaryGradeName = x.SalaryGrade?.SalaryGradeName,
                EmployeeGradeId = x.EmployeeGradeId,
                EmployeeGradeName = x.EmployeeGrade?.GradeName,
                PayrollPeriodId = x.PayrollPeriodId,
                PayrollPeriodCode = x.PayrollPeriod?.PayrollPeriodCode,
                PayrollPeriodName = x.PayrollPeriod?.PayrollPeriodName,
                // Tanpa ViewAmount nominal null (tersembunyi), tidak pernah 0: nol adalah angka sungguhan dan akan menyesatkan.
                BaseSalary = includeAmounts ? x.BaseSalary : null,
                AmountsVisible = includeAmounts,
                GradeMinimumSalary = includeAmounts ? range.GradeMinimumSalary : null,
                GradeMaximumSalary = includeAmounts ? range.GradeMaximumSalary : null,
                StructureMinimumSalary = includeAmounts ? range.StructureMinimumSalary : null,
                StructureMaximumSalary = includeAmounts ? range.StructureMaximumSalary : null,
                EffectiveMinimumSalary = includeAmounts ? range.EffectiveMinimumSalary : null,
                EffectiveMaximumSalary = includeAmounts ? range.EffectiveMaximumSalary : null,
                SalaryRangeText = includeAmounts ? range.SalaryRangeText : "Terselubung (Memerlukan Wewenang ViewAmount)",
                OperationalStatus = statusText,
                CurrencyCode = x.CurrencyCode,
                PaymentFrequency = x.PaymentFrequency,
                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,
                IsPrimary = x.IsPrimary,
                IsConfidential = x.IsConfidential,
                IsActive = x.IsActive,
                ApprovedByUserId = x.ApprovedByUserId,
                ApprovedByUserName = x.ApprovedByUserId.HasValue ? GetActorName(actorNames, x.ApprovedByUserId.Value) : null,
                ApprovedAt = x.ApprovedAt,
                // Satu aturan: IsActive tidak pernah berarti disetujui (P0-02).
                IsApproved = approved,
                ApprovalStatus = WfpSalaryAssignmentRules.StatusOf(x.ApprovedAt, x.ApprovedByUserId),
                ApprovalRequired = approvalRequired,
                // Kelayakan Payroll, terpisah dari fakta persetujuan di atas. AutoByPolicy tidak mengisi ApprovedAt/ApprovedByUserId.
                AuthorizationStatus = standing.AuthorizationStatus,
                OperationalStatusCode = standing.OperationalStatus,
                IsPayrollUsable = standing.IsPayrollUsable,
                PersonnelActionOrderId = x.PersonnelActionOrderId,
                PersonnelActionOrderNumber = x.PersonnelActionOrder?.ActionOrderNumber,
                IsLockedByFinalPayroll = lastFinalPayrollPeriodEnd.HasValue,
                LastFinalPayrollPeriodEnd = lastFinalPayrollPeriodEnd,
                StructureDefaultBaseSalary = includeAmounts ? x.SalaryStructure?.DefaultBaseSalary : null,
                BaseSalaryDiffersFromStructureDefault = differsFromStructureDefault,
                MasterValidityStatus = validity.Code,
                MasterValidityMessage = validity.Message,
                Description = x.Description,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = GetActorName(actorNames, x.CreateBy),
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                UpdateByName = GetActorName(actorNames, x.UpdateBy)
            };
        }

        private async Task<Dictionary<Guid, string?>> GetActorNamesAsync(IEnumerable<Guid> ids, CancellationToken ct)
        {
            var actorIds = ids.Where(x => x != Guid.Empty).Distinct().ToList();
            if (!actorIds.Any()) return new Dictionary<Guid, string?>();
            return await _dbContext.Users.AsNoTracking().Where(x => actorIds.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode })
                .ToDictionaryAsync(x => x.Id, x => (string?)x.Name, ct);
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> map, Guid id) => id == Guid.Empty ? null : map.TryGetValue(id, out var name) ? name : null;
        private Guid GetCurrentUserId() { var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id"); return Guid.TryParse(value, out var id) ? id : Guid.Empty; }
        private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static Guid? NormalizeGuid(Guid? value) => !value.HasValue || value.Value == Guid.Empty ? null : value.Value;
        private static string NormalizeFrequency(string value) => PaymentFrequencies.FirstOrDefault(x => x.Equals(value?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? value?.Trim() ?? string.Empty;
        private static void NormalizePaging(ref int pageNumber, ref int pageSize) { pageNumber = pageNumber <= 0 ? 1 : pageNumber; pageSize = pageSize <= 0 ? 25 : Math.Min(pageSize, 100); }

        private async Task<bool> ExistsActiveAsync<TEntity>(Guid id, CancellationToken ct) where TEntity : IdentityModel =>
            await _dbContext.Set<TEntity>().AsNoTracking().AnyAsync(x => EF.Property<Guid>(x, "Id") == id && EF.Property<bool>(x, "IsActive") && !EF.Property<bool>(x, "IsDelete"), ct);

        private async Task<bool> ExistsActiveIfProvidedAsync<TEntity>(Guid? id, CancellationToken ct) where TEntity : IdentityModel =>
            !id.HasValue || id.Value == Guid.Empty || await ExistsActiveAsync<TEntity>(id.Value, ct);

        private static DateRangeResult ResolveDateRange(DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            if (!string.IsNullOrWhiteSpace(customPeriod) && !customPeriod.Equals("custom", StringComparison.OrdinalIgnoreCase))
            {
                var today = DateTime.UtcNow.Date;
                return customPeriod.Trim().ToLowerInvariant() switch
                {
                    "today" => DateRangeResult.Valid(today, today.AddDays(1)),
                    "last7days" => DateRangeResult.Valid(today.AddDays(-6), today.AddDays(1)),
                    "last30days" => DateRangeResult.Valid(today.AddDays(-29), today.AddDays(1)),
                    "thismonth" => DateRangeResult.Valid(new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
                    _ => DateRangeResult.Invalid("Custom period tidak dikenali.")
                };
            }
            DateTime? start = startDate.HasValue ? DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc) : null;
            DateTime? endExclusive = endDate.HasValue ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc) : null;
            if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value) return DateRangeResult.Invalid("StartDate tidak boleh lebih besar atau sama dengan EndDate.");
            return DateRangeResult.Valid(start, endExclusive);
        }

        private static List<WfpSalaryAssignmentStringOptionResponse> BuildPeriods() => new()
        {
            new() { Value = "custom", Label = "Custom" },
            new() { Value = "today", Label = "Hari ini" },
            new() { Value = "last7days", Label = "7 hari terakhir" },
            new() { Value = "last30days", Label = "30 hari terakhir" },
            new() { Value = "thismonth", Label = "Bulan ini" }
        };

        private sealed class DateRangeResult
        {
            public bool IsValid { get; private init; }
            public DateTime? Start { get; private init; }
            public DateTime? EndExclusive { get; private init; }
            public string? ErrorMessage { get; private init; }
            public static DateRangeResult Valid(DateTime? start, DateTime? end) => new() { IsValid = true, Start = start, EndExclusive = end };
            public static DateRangeResult Invalid(string message) => new() { IsValid = false, ErrorMessage = message };
        }
    }
}
