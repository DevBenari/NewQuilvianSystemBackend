using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// PAY-BE-19 (blueprint payroll-gaji PGJ-C1, PRL-DEC-07/10/31/38/43): penghasilan variabel manual — insentif, bonus, dan honor
/// tugas tambahan — sebagai PrlPayrollVariableInput dengan SourceType = PayrollManualInput. Sub-proses transaksi yang ter-scope
/// ke run induk; tidak ada hapus, pembatalan lewat aksi cancel. Aturan: validation-matrix.md VI-01..VI-09 dan VI-12;
/// transisi: state-transition-matrix.md bagian 1. Nomor PVI-yyyyMM-nnnn dari NumberSeriesAllocator (QBE-CODE-002/003/006).
/// Pengaju boleh menyetujui inputnya sendiri (PRL-DEC-10, 43). Nominal tidak pernah ditulis ke log.
/// </summary>
public sealed class PayrollVariableInputService
{
    private const string LogCategory = "Corporate.PayrollManagement.PayrollVariableInput";
    private const string SequenceKey = "PRL_VARIABLE_INPUT";
    private const string NumberPrefix = "PVI";
    private const int MaxPageSize = 200;
    private const int NotesMaxLength = 1000;
    private const int AttachmentMaxLength = 500;
    private const int ReasonMaxLength = 500;

    private const string RunClosedMessage = "Run ini sudah tidak menerima penghasilan variabel.";
    private const string VersionConflictMessage = "Data ini baru saja diubah petugas lain. Muat ulang lalu coba lagi.";
    private const string AmountMessage = "Nominal harus lebih dari nol.";
    private const string InputTypeMessage = "Jenis input harus Insentif, Bonus, atau Honor Tugas Tambahan.";
    private const string HonorariumNotesMessage = "Honor tugas tambahan wajib menyebut tugasnya.";
    private const string ReasonRequiredMessage = "Alasan wajib diisi.";
    private const string NotFoundMessage = "Penghasilan variabel tidak ditemukan pada run ini.";

    /// <summary>Aktor cadangan untuk stempel deret nomor bila identitas petugas kosong (sama dengan modul lain).</summary>
    private static readonly Guid SystemFallbackUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private readonly ApplicationDbContext _dbContext;
    private readonly NumberSeriesAllocator _numberSeriesAllocator;
    private readonly LoggerService _loggerService;

    public PayrollVariableInputService(
        ApplicationDbContext dbContext,
        NumberSeriesAllocator numberSeriesAllocator,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _numberSeriesAllocator = numberSeriesAllocator;
        _loggerService = loggerService;
    }

    /// <summary>Aksi petugas pada satu input: status asal yang sah, status tujuan, dan kata kerja untuk pesan VI-09.</summary>
    private sealed record ManualAction(string Name, string Target, string Verb, string[] From, bool RequiresReason);

    private static readonly ManualAction SubmitAction = new("Submit", PayrollManualInputStatuses.Submitted, "diajukan",
        new[] { PayrollManualInputStatuses.Draft }, RequiresReason: false);
    private static readonly ManualAction ApproveAction = new("Approve", PayrollManualInputStatuses.Approved, "disetujui",
        new[] { PayrollManualInputStatuses.Submitted }, RequiresReason: false);
    private static readonly ManualAction RejectAction = new("Reject", PayrollManualInputStatuses.Rejected, "ditolak",
        new[] { PayrollManualInputStatuses.Submitted }, RequiresReason: true);
    private static readonly ManualAction CancelAction = new("Cancel", PayrollManualInputStatuses.Cancelled, "dibatalkan",
        new[] { PayrollManualInputStatuses.Draft, PayrollManualInputStatuses.Submitted, PayrollManualInputStatuses.Approved }, RequiresReason: true);

    // ------------------------------------------------------------------------------------
    // Baca
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<PayrollVariableInputResponse>> ListAsync(
        Guid runId, PayrollVariableInputQuery query, CancellationToken cancellationToken)
    {
        var run = await LoadRunAsync(runId, cancellationToken);
        var source = ManualInputs(runId).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.InputStatus))
        {
            var status = Canonical(PayrollManualInputStatuses.All, query.InputStatus) ?? query.InputStatus.Trim();
            source = source.Where(x => x.InputStatus == status);
        }

        if (!string.IsNullOrWhiteSpace(query.InputType))
        {
            var type = Canonical(PayrollManualInputTypes.All, query.InputType) ?? query.InputType.Trim();
            source = source.Where(x => x.InputType == type);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            source = source.Where(x =>
                x.InputNumber.ToLower().Contains(term)
                || x.PayrollRunEmployee!.EmployeeNameSnapshot.ToLower().Contains(term)
                || (x.PayrollRunEmployee.EmployeeNumberSnapshot != null && x.PayrollRunEmployee.EmployeeNumberSnapshot.ToLower().Contains(term)));
        }

        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);
        var total = await source.CountAsync(cancellationToken);

        var items = await Project(source
                .OrderByDescending(x => x.CreateDateTime)
                .ThenBy(x => x.InputNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize))
            .ToListAsync(cancellationToken);
        foreach (var item in items) item.AvailableActions = AvailableActions(run.RunStatus, item.InputStatus);

        return new PagedResult<PayrollVariableInputResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    public async Task<PayrollVariableInputResponse> GetAsync(Guid runId, Guid id, CancellationToken cancellationToken)
    {
        var run = await LoadRunAsync(runId, cancellationToken);
        return await GetProjectedAsync(runId, run.RunStatus, id, cancellationToken);
    }

    // ------------------------------------------------------------------------------------
    // Buat, impor, ubah isi
    // ------------------------------------------------------------------------------------

    public async Task<PayrollVariableInputResponse> CreateAsync(
        Guid runId, CreatePayrollVariableInputRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var run = await LoadRunAsync(runId, cancellationToken);
        EnsureAcceptsInputs(run.RunStatus);

        // VI-02: pegawai harus terdaftar di run ini.
        var runEmployee = await _dbContext.PrlPayrollRunEmployees.AsNoTracking()
            .Where(x => x.Id == request.PayrollRunEmployeeId && x.PayrollRunId == runId && !x.IsDelete)
            .Select(x => new { x.Id, x.CurrencyCode })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ArgumentException($"Pegawai {request.PayrollRunEmployeeId} tidak ada di run periode ini.");

        var content = await ValidateContentAsync(request.PayrollComponentId, request.InputType, request.Amount, request.Notes,
            request.AttachmentPath, cancellationToken);

        var now = DateTime.UtcNow;
        var entity = NewInput(runEmployee.Id, runEmployee.CurrencyCode, content.ComponentId, content.InputType, request.Amount,
            content.Notes, content.AttachmentPath, await AllocateNumberAsync(actorUserId, cancellationToken), actorUserId, now);

        _dbContext.PrlPayrollVariableInputs.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "PayrollVariableInput.Create",
            "Penghasilan variabel manual dibuat.",
            new { EntityId = entity.Id, PayrollRunId = runId, entity.InputNumber, entity.InputType, Status = entity.InputStatus, ActorUserId = actorUserId });

        return await GetProjectedAsync(runId, run.RunStatus, entity.Id, cancellationToken);
    }

    /// <summary>
    /// VI-08: impor semua atau tidak sama sekali. Seluruh baris divalidasi lebih dulu; satu baris gagal membatalkan impor dan
    /// tidak ada nomor yang diterbitkan. Bila semua sah, nomor dialokasikan lalu seluruh baris disimpan dalam satu SaveChanges
    /// (satu transaksi database).
    /// </summary>
    public async Task<PayrollVariableInputImportResult> ImportAsync(
        Guid runId, ImportPayrollVariableInputRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var run = await LoadRunAsync(runId, cancellationToken);
        EnsureAcceptsInputs(run.RunStatus);

        var rows = request.Rows ?? new List<ImportPayrollVariableInputRow>();
        if (rows.Count == 0) throw new ArgumentException("Tidak ada baris untuk diimpor.");

        var runEmployees = (await _dbContext.PrlPayrollRunEmployees.AsNoTracking()
                .Where(x => x.PayrollRunId == runId && !x.IsDelete && x.EmployeeNumberSnapshot != null)
                .Select(x => new { x.Id, x.CurrencyCode, x.EmployeeNumberSnapshot })
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.EmployeeNumberSnapshot!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        var codes = rows.Select(r => r.PayrollComponentCode?.Trim().ToUpper())
            .Where(c => !string.IsNullOrEmpty(c)).Select(c => c!).Distinct().ToList();
        var components = (await _dbContext.MstPayrollComponents.AsNoTracking()
                .Where(x => !x.IsDelete && codes.Contains(x.PayrollComponentCode.ToUpper()))
                .Select(x => new ComponentInfo(x.Id, x.PayrollComponentCode, x.IsActive, x.ComponentType))
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.Code.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var failed = new List<PayrollVariableInputImportFailedRow>();
        var valid = new List<(Guid RunEmployeeId, string CurrencyCode, Guid ComponentId, string InputType, decimal Amount, string? Notes)>();
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var reasons = new List<string>();

            var employeeNumber = row.EmployeeNumber?.Trim();
            (Guid Id, string CurrencyCode)? runEmployee = null;
            if (string.IsNullOrEmpty(employeeNumber) || !runEmployees.TryGetValue(employeeNumber, out var matches))
                reasons.Add($"Pegawai {(string.IsNullOrEmpty(employeeNumber) ? "(kosong)" : employeeNumber)} tidak ada di run periode ini.");
            else if (matches.Count > 1)
                reasons.Add($"Pegawai {employeeNumber} tercatat lebih dari sekali di run ini. Masukkan lewat form satu per satu.");
            else
                runEmployee = (matches[0].Id, matches[0].CurrencyCode);

            var code = row.PayrollComponentCode?.Trim();
            components.TryGetValue(code ?? string.Empty, out var component);
            var componentReason = ComponentReason(component, string.IsNullOrEmpty(code) ? "(kosong)" : code);
            if (componentReason is not null) reasons.Add(componentReason);

            reasons.AddRange(ContentReasons(row.InputType, row.Amount, row.Notes, attachmentPath: null, out var inputType, out var notes));

            if (reasons.Count > 0)
            {
                failed.Add(new PayrollVariableInputImportFailedRow { RowNumber = i + 1, Reason = string.Join(" ", reasons) });
                continue;
            }

            valid.Add((runEmployee!.Value.Id, runEmployee.Value.CurrencyCode, component!.Id, inputType!, row.Amount, notes));
        }

        if (failed.Count > 0)
        {
            throw new PayrollVariableInputImportException(
                $"Impor dibatalkan. {failed.Count} baris bermasalah; tidak ada baris yang tersimpan.",
                new PayrollVariableInputImportResult { CreatedCount = 0, FailedRows = failed });
        }

        var now = DateTime.UtcNow;
        var entities = new List<PrlPayrollVariableInput>(valid.Count);
        foreach (var item in valid)
        {
            entities.Add(NewInput(item.RunEmployeeId, item.CurrencyCode, item.ComponentId, item.InputType, item.Amount, item.Notes,
                attachmentPath: null, await AllocateNumberAsync(actorUserId, cancellationToken), actorUserId, now));
        }

        _dbContext.PrlPayrollVariableInputs.AddRange(entities);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "PayrollVariableInput.Import",
            "Penghasilan variabel manual diimpor.",
            new { PayrollRunId = runId, CreatedCount = entities.Count, Status = "Imported", ActorUserId = actorUserId });

        return new PayrollVariableInputImportResult { CreatedCount = entities.Count };
    }

    public async Task<PayrollVariableInputResponse> UpdateContentAsync(
        Guid runId, Guid id, UpdatePayrollVariableInputContentRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var run = await LoadRunAsync(runId, cancellationToken);
        var entity = await FindTrackedAsync(runId, id, cancellationToken);

        EnsureAcceptsInputs(run.RunStatus);
        if (!Is(entity.InputStatus, PayrollManualInputStatuses.Draft))
            throw InvalidTransition(entity.InputStatus, "diubah");
        EnsureVersion(request.ExpectedRowVersion, entity);

        var content = await ValidateContentAsync(request.PayrollComponentId, request.InputType, request.Amount, request.Notes,
            request.AttachmentPath, cancellationToken);

        entity.PayrollComponentId = content.ComponentId;
        entity.InputType = content.InputType;
        entity.Amount = request.Amount;
        entity.Rate = request.Amount;
        entity.Quantity = 1m;
        entity.Notes = content.Notes;
        entity.AttachmentPath = content.AttachmentPath;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "PayrollVariableInput.Update",
            "Isi penghasilan variabel manual diperbarui.",
            new { EntityId = entity.Id, PayrollRunId = runId, entity.InputNumber, entity.InputType, Status = entity.InputStatus, ActorUserId = actorUserId });

        return await GetProjectedAsync(runId, run.RunStatus, entity.Id, cancellationToken);
    }

    // ------------------------------------------------------------------------------------
    // Aksi status
    // ------------------------------------------------------------------------------------

    public Task<PayrollVariableInputResponse> SubmitAsync(Guid runId, Guid id, PayrollVariableInputActionRequest request, Guid actorUserId, CancellationToken cancellationToken) =>
        TransitionAsync(runId, id, SubmitAction, request.ExpectedRowVersion, reason: null, actorUserId, cancellationToken);

    public Task<PayrollVariableInputResponse> ApproveAsync(Guid runId, Guid id, PayrollVariableInputActionRequest request, Guid actorUserId, CancellationToken cancellationToken) =>
        TransitionAsync(runId, id, ApproveAction, request.ExpectedRowVersion, reason: null, actorUserId, cancellationToken);

    public Task<PayrollVariableInputResponse> RejectAsync(Guid runId, Guid id, PayrollVariableInputDecisionRequest request, Guid actorUserId, CancellationToken cancellationToken) =>
        TransitionAsync(runId, id, RejectAction, request.ExpectedRowVersion, request.Reason, actorUserId, cancellationToken);

    public Task<PayrollVariableInputResponse> CancelAsync(Guid runId, Guid id, PayrollVariableInputDecisionRequest request, Guid actorUserId, CancellationToken cancellationToken) =>
        TransitionAsync(runId, id, CancelAction, request.ExpectedRowVersion, request.Reason, actorUserId, cancellationToken);

    public Task<PayrollVariableInputBulkResult> BulkSubmitAsync(Guid runId, PayrollVariableInputBulkRequest request, Guid actorUserId, CancellationToken cancellationToken) =>
        BulkAsync(runId, request, SubmitAction, actorUserId, cancellationToken);

    public Task<PayrollVariableInputBulkResult> BulkApproveAsync(Guid runId, PayrollVariableInputBulkRequest request, Guid actorUserId, CancellationToken cancellationToken) =>
        BulkAsync(runId, request, ApproveAction, actorUserId, cancellationToken);

    private async Task<PayrollVariableInputResponse> TransitionAsync(
        Guid runId, Guid id, ManualAction action, string? expectedRowVersion, string? reason, Guid actorUserId, CancellationToken cancellationToken)
    {
        var run = await LoadRunAsync(runId, cancellationToken);
        var entity = await FindTrackedAsync(runId, id, cancellationToken);

        var changed = await ApplyTransitionAsync(run.RunStatus, entity, action, expectedRowVersion, reason, actorUserId, DateTime.UtcNow, cancellationToken);
        if (changed)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditTransitionAsync(runId, entity, action, actorUserId);
        }

        return await GetProjectedAsync(runId, run.RunStatus, entity.Id, cancellationToken);
    }

    /// <summary>
    /// Aksi massal: tiap input diproses dan disimpan sendiri-sendiri; satu input gagal tidak menggagalkan input lain
    /// (api-contract.md). Tanpa token versi karena permintaan hanya membawa daftar id.
    /// </summary>
    private async Task<PayrollVariableInputBulkResult> BulkAsync(
        Guid runId, PayrollVariableInputBulkRequest request, ManualAction action, Guid actorUserId, CancellationToken cancellationToken)
    {
        var run = await LoadRunAsync(runId, cancellationToken);
        EnsureAcceptsInputs(run.RunStatus);

        var ids = (request.InputIds ?? new List<Guid>()).Where(x => x != Guid.Empty).Distinct().ToList();
        if (ids.Count == 0) throw new ArgumentException("Pilih minimal satu penghasilan variabel.");

        var entities = await ManualInputs(runId)
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var result = new PayrollVariableInputBulkResult();
        var changedCount = 0;
        foreach (var id in ids)
        {
            if (!entities.TryGetValue(id, out var entity))
            {
                result.Failed.Add(new PayrollVariableInputBulkFailure { InputId = id, Reason = NotFoundMessage });
                continue;
            }

            try
            {
                // Seluruh penolakan terjadi sebelum entity diubah, sehingga input yang gagal tidak ikut tersimpan.
                if (await ApplyTransitionAsync(run.RunStatus, entity, action, expectedRowVersion: null, reason: null, actorUserId, DateTime.UtcNow, cancellationToken))
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    changedCount++;
                }
                result.Succeeded.Add(id);
            }
            catch (Exception ex) when (ex is ArgumentException or PayrollRunConflictException or PayrollVersionConflictException)
            {
                result.Failed.Add(new PayrollVariableInputBulkFailure { InputId = id, Reason = ex.Message });
            }
        }

        await _loggerService.AuditAsync(LogCategory, $"PayrollVariableInput.Bulk{action.Name}",
            "Aksi massal penghasilan variabel manual.",
            new { PayrollRunId = runId, Requested = ids.Count, Succeeded = result.Succeeded.Count, Changed = changedCount, Failed = result.Failed.Count, Status = action.Target, ActorUserId = actorUserId });

        return result;
    }

    /// <summary>
    /// Menerapkan satu transisi. Mengembalikan false bila input sudah berstatus tujuan (idempoten, tanpa perubahan).
    /// Urutan penjaga: VI-01 (run menerima input), VI-09 (transisi sah), VI-12 (token versi), VI-07 (alasan), lalu isi lengkap
    /// untuk pengajuan. Tidak ada field yang diubah sebelum seluruh penjaga lolos.
    /// </summary>
    private async Task<bool> ApplyTransitionAsync(
        string runStatus, PrlPayrollVariableInput entity, ManualAction action, string? expectedRowVersion, string? reason,
        Guid actorUserId, DateTime now, CancellationToken cancellationToken)
    {
        if (Is(entity.InputStatus, action.Target)) return false;

        EnsureAcceptsInputs(runStatus);
        if (!action.From.Any(s => Is(entity.InputStatus, s)))
            throw InvalidTransition(entity.InputStatus, action.Verb);
        EnsureVersion(expectedRowVersion, entity);

        string? normalizedReason = null;
        if (action.RequiresReason)
        {
            normalizedReason = NormalizeText(reason) ?? throw new ArgumentException(ReasonRequiredMessage);
            if (normalizedReason.Length > ReasonMaxLength)
                throw new ArgumentException($"Alasan paling panjang {ReasonMaxLength} karakter.");
        }

        if (action == SubmitAction)
        {
            // "Isi lengkap" (state-transition-matrix.md): isi diperiksa ulang karena komponen bisa dinonaktifkan sesudah input dibuat.
            await ValidateContentAsync(entity.PayrollComponentId, entity.InputType, entity.Amount, entity.Notes, entity.AttachmentPath, cancellationToken);
        }

        var actor = actorUserId == Guid.Empty ? (Guid?)null : actorUserId;
        if (action == SubmitAction)
        {
            entity.SubmittedAt = now;
            entity.SubmittedByUserId = actor;
        }
        else if (action == ApproveAction)
        {
            entity.ApprovedAt = now;
            entity.ApprovedByUserId = actor;
        }
        else if (action == RejectAction)
        {
            entity.RejectedAt = now;
            entity.RejectedByUserId = actor;
            entity.DecisionReason = normalizedReason;
        }
        else if (action == CancelAction)
        {
            entity.IsCancel = true;
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            entity.DecisionReason = normalizedReason;
        }

        entity.InputStatus = action.Target;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;
        return true;
    }

    private Task AuditTransitionAsync(Guid runId, PrlPayrollVariableInput entity, ManualAction action, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, $"PayrollVariableInput.{action.Name}",
            $"Penghasilan variabel manual {action.Verb}.",
            new { EntityId = entity.Id, PayrollRunId = runId, entity.InputNumber, Status = entity.InputStatus, ActorUserId = actorUserId });

    // ------------------------------------------------------------------------------------
    // Validasi isi (VI-03..VI-06)
    // ------------------------------------------------------------------------------------

    private sealed record ComponentInfo(Guid Id, string Code, bool IsActive, string ComponentType);

    private sealed record ValidContent(Guid ComponentId, string InputType, string? Notes, string? AttachmentPath);

    private async Task<ValidContent> ValidateContentAsync(
        Guid componentId, string? inputType, decimal amount, string? notes, string? attachmentPath, CancellationToken cancellationToken)
    {
        var reasons = ContentReasons(inputType, amount, notes, attachmentPath, out var normalizedType, out var normalizedNotes);
        if (reasons.Count > 0) throw new ArgumentException(reasons[0]);

        var component = await _dbContext.MstPayrollComponents.AsNoTracking()
            .Where(x => x.Id == componentId && !x.IsDelete)
            .Select(x => new ComponentInfo(x.Id, x.PayrollComponentCode, x.IsActive, x.ComponentType))
            .FirstOrDefaultAsync(cancellationToken);
        var componentReason = ComponentReason(component, component?.Code ?? componentId.ToString());
        if (componentReason is not null) throw new ArgumentException(componentReason);

        return new ValidContent(component!.Id, normalizedType!, normalizedNotes, NormalizeText(attachmentPath));
    }

    /// <summary>VI-03: komponen harus ada, aktif, dan bertipe Earning (penghasilan).</summary>
    private static string? ComponentReason(ComponentInfo? component, string code) =>
        component is null || !component.IsActive || !string.Equals(component.ComponentType, "Earning", StringComparison.OrdinalIgnoreCase)
            ? $"Komponen {code} tidak aktif atau bukan jenis penghasilan."
            : null;

    /// <summary>VI-05, VI-04, VI-06 serta batas panjang kolom. Mengembalikan seluruh sebab (dipakai impor) dalam urutan tetap.</summary>
    private static List<string> ContentReasons(
        string? inputType, decimal amount, string? notes, string? attachmentPath, out string? normalizedType, out string? normalizedNotes)
    {
        var reasons = new List<string>();
        normalizedType = Canonical(PayrollManualInputTypes.All, inputType);
        normalizedNotes = NormalizeText(notes);

        if (normalizedType is null) reasons.Add(InputTypeMessage);
        if (amount <= 0m) reasons.Add(AmountMessage);
        if (normalizedType == PayrollManualInputTypes.AdditionalDutyHonorarium && normalizedNotes is null) reasons.Add(HonorariumNotesMessage);
        if (normalizedNotes is { Length: > NotesMaxLength }) reasons.Add($"Catatan paling panjang {NotesMaxLength} karakter.");
        if (NormalizeText(attachmentPath) is { Length: > AttachmentMaxLength }) reasons.Add($"Lokasi lampiran paling panjang {AttachmentMaxLength} karakter.");
        return reasons;
    }

    // ------------------------------------------------------------------------------------
    // Pembantu
    // ------------------------------------------------------------------------------------

    private IQueryable<PrlPayrollVariableInput> ManualInputs(Guid runId) =>
        _dbContext.PrlPayrollVariableInputs.Where(x =>
            !x.IsDelete
            && x.SourceType == PayrollVariableInputSources.ManualInput
            && x.PayrollRunEmployee != null
            && x.PayrollRunEmployee.PayrollRunId == runId);

    private sealed record RunInfo(Guid Id, string RunStatus);

    private async Task<RunInfo> LoadRunAsync(Guid runId, CancellationToken cancellationToken) =>
        await _dbContext.PrlPayrollRuns.AsNoTracking()
            .Where(x => x.Id == runId && !x.IsDelete)
            .Select(x => new RunInfo(x.Id, x.RunStatus))
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new KeyNotFoundException("Payroll run tidak ditemukan.");

    private async Task<PrlPayrollVariableInput> FindTrackedAsync(Guid runId, Guid id, CancellationToken cancellationToken) =>
        await ManualInputs(runId).FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
        ?? throw new KeyNotFoundException(NotFoundMessage);

    private async Task<PayrollVariableInputResponse> GetProjectedAsync(Guid runId, string runStatus, Guid id, CancellationToken cancellationToken)
    {
        var item = await Project(ManualInputs(runId).AsNoTracking().Where(x => x.Id == id)).FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException(NotFoundMessage);
        item.AvailableActions = AvailableActions(runStatus, item.InputStatus);
        return item;
    }

    private static IQueryable<PayrollVariableInputResponse> Project(IQueryable<PrlPayrollVariableInput> query) =>
        query.Select(x => new PayrollVariableInputResponse
        {
            Id = x.Id,
            InputNumber = x.InputNumber,
            PayrollRunEmployeeId = x.PayrollRunEmployeeId,
            EmployeeNumber = x.PayrollRunEmployee != null ? x.PayrollRunEmployee.EmployeeNumberSnapshot ?? string.Empty : string.Empty,
            EmployeeName = x.PayrollRunEmployee != null ? x.PayrollRunEmployee.EmployeeNameSnapshot : string.Empty,
            PayrollComponentId = x.PayrollComponentId,
            PayrollComponentCode = x.PayrollComponent != null ? x.PayrollComponent.PayrollComponentCode : string.Empty,
            PayrollComponentName = x.PayrollComponent != null ? x.PayrollComponent.PayrollComponentName : string.Empty,
            InputType = x.InputType,
            InputStatus = x.InputStatus,
            Amount = x.Amount,
            Notes = x.Notes,
            AttachmentPath = x.AttachmentPath,
            SubmittedAt = x.SubmittedAt,
            SubmittedByName = x.SubmittedByUser != null ? x.SubmittedByUser.DisplayName : null,
            ApprovedAt = x.ApprovedAt,
            ApprovedByName = x.ApprovedByUser != null ? x.ApprovedByUser.DisplayName : null,
            RejectedAt = x.RejectedAt,
            RejectedByName = x.RejectedByUser != null ? x.RejectedByUser.DisplayName : null,
            DecisionReason = x.DecisionReason,
            // Dihitung di sisi klien pada proyeksi akhir; sama persis dengan token yang diperiksa EnsureVersion.
            RowVersion = PayrollVersionToken.From(x.CreateDateTime, x.UpdateDateTime)
        });

    /// <summary>Aksi yang sah menurut state-transition-matrix.md bagian 1; kosong bila run tidak lagi menerima input.</summary>
    private static List<string> AvailableActions(string runStatus, string inputStatus)
    {
        if (!PayrollRunStatuses.AcceptsInputs(runStatus)) return new List<string>();
        if (Is(inputStatus, PayrollManualInputStatuses.Draft)) return new List<string> { "update", "submit", "cancel" };
        if (Is(inputStatus, PayrollManualInputStatuses.Submitted)) return new List<string> { "approve", "reject", "cancel" };
        if (Is(inputStatus, PayrollManualInputStatuses.Approved)) return new List<string> { "cancel" };
        return new List<string>();
    }

    private PrlPayrollVariableInput NewInput(
        Guid runEmployeeId, string currencyCode, Guid componentId, string inputType, decimal amount, string? notes,
        string? attachmentPath, string inputNumber, Guid actorUserId, DateTime now) =>
        new()
        {
            Id = Guid.NewGuid(),
            PayrollRunEmployeeId = runEmployeeId,
            PayrollComponentId = componentId,
            InputNumber = inputNumber,
            InputDate = DateOnly.FromDateTime(now),
            InputType = inputType,
            InputStatus = PayrollManualInputStatuses.Draft,
            CurrencyCode = string.IsNullOrWhiteSpace(currencyCode) ? "IDR" : currencyCode,
            Quantity = 1m,
            Rate = amount,
            Amount = amount,
            SourceType = PayrollVariableInputSources.ManualInput,
            SourceId = null,
            AttachmentPath = attachmentPath,
            Notes = notes,
            IsActive = true,
            CreateDateTime = now,
            CreateBy = actorUserId,
            IsDelete = false,
            IsCancel = false
        };

    /// <summary>Nomor PVI-yyyyMM-nnnn, deret bulanan. Nomor yang sudah terbit tidak kembali bila simpan gagal (celah diterima).</summary>
    private async Task<string> AllocateNumberAsync(Guid actorUserId, CancellationToken cancellationToken)
    {
        var request = new NumberAllocationRequest(
            SequenceKey: SequenceKey,
            Prefix: NumberPrefix,
            ResetPolicy: NumberSeriesResetPolicies.Monthly,
            SequenceDigits: 4,
            ActorUserId: actorUserId == Guid.Empty ? SystemFallbackUserId : actorUserId,
            Instant: DateTimeOffset.UtcNow,
            MinimumValue: 0,
            CompactScopeFormat: true,
            AllowSequenceOverflow: true);

        try
        {
            return await _numberSeriesAllocator.AllocateAsync(request, cancellationToken);
        }
        catch (NumberSeriesAllocationException ex)
        {
            throw new PayrollRunConflictException($"Nomor penghasilan variabel tidak dapat diterbitkan: {ex.Message}");
        }
    }

    private static void EnsureAcceptsInputs(string runStatus)
    {
        if (!PayrollRunStatuses.AcceptsInputs(runStatus)) throw new ArgumentException(RunClosedMessage);
    }

    /// <summary>VI-12. Token kosong berarti klien tidak meminta pemeriksaan, sama dengan PayrollVersionToken.EnsureMatches.</summary>
    private static void EnsureVersion(string? expected, PrlPayrollVariableInput entity)
    {
        if (string.IsNullOrWhiteSpace(expected)) return;
        if (!string.Equals(expected.Trim(), PayrollVersionToken.From(entity.CreateDateTime, entity.UpdateDateTime), StringComparison.Ordinal))
            throw new PayrollVersionConflictException(VersionConflictMessage);
    }

    /// <summary>VI-09 (409): "Input berstatus {status} tidak dapat {aksi}."</summary>
    private static PayrollRunConflictException InvalidTransition(string status, string verb) =>
        new($"Input berstatus {StatusLabel(status)} tidak dapat {verb}.");

    private static string StatusLabel(string status) => Canonical(PayrollManualInputStatuses.All, status) switch
    {
        PayrollManualInputStatuses.Draft => "Draf",
        PayrollManualInputStatuses.Submitted => "Diajukan",
        PayrollManualInputStatuses.Approved => "Disetujui",
        PayrollManualInputStatuses.Rejected => "Ditolak",
        PayrollManualInputStatuses.Cancelled => "Dibatalkan",
        PayrollManualInputStatuses.Applied => "Diterapkan",
        _ => status
    };

    private static string? Canonical(IReadOnlyList<string> values, string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : values.FirstOrDefault(x => string.Equals(x, value.Trim(), StringComparison.OrdinalIgnoreCase));

    private static bool Is(string? value, string expected) => string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);

    private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
