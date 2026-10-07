using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using static QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services.OperatingRoomCommandSupport;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;

/// <summary>
/// Catatan Pra-Operasi bangsal berversi (<c>BE-RWI-176</c>, kontrak <c>0.10.0</c> API 11.3,
/// state matrix 9.2, <c>RWI-DEC-173</c>, <c>RWI-DEC-174</c>, <c>RWI-DEC-199</c>).
/// </summary>
/// <remarks>
/// <para>
/// Perawat bangsal menyimpan draf dan mengirim; perawat OK mengonfirmasi dari akun berbeda. Saat
/// dikirim, tanda vital dan nyeri terakhir <b>dibekukan</b> sebagai potret — tidak diketik ulang.
/// Kasus hanya dapat "Siap" bila versi terbaru <c>Confirmed</c> (<c>INV-RWF-26</c>) dan sisi
/// penandaan cocok dengan sisi pesanan (<c>INV-RWF-27</c>).
/// </para>
/// <para>
/// Contoh <c>UAT-RWF-21</c>: versi 1 <c>Confirmed</c>; kasus ditunda → versi 1 <c>NeedsUpdate</c>;
/// <c>PUT ward-pre-op/draft</c> → versi 2 <c>Draft</c> dengan butir versi 1 sebagai usulan;
/// <c>PATCH send</c> → potret TD 160/100 dari pencatatan terbaru dan versi 1 <c>Superseded</c>.
/// </para>
/// </remarks>
public sealed class OprWardPreOpService
{
    public const string LateralityMismatchCode = "OPR-WPO-001";
    public const string SameAccountCode = "OPR-WPO-002";
    public const string MandatoryItemsIncompleteCode = "OPR-WPO-003";
    public const string CaseLockedCode = "OPR-WPO-004";
    public const string VitalSignMissingCode = "OPR-WPO-005";

    /// <summary>Kode <c>Blockers[]</c> gerbang "Siap" (API 11.3).</summary>
    public const string BlockerIncomplete = "WARD_PRE_OP_INCOMPLETE";
    public const string BlockerNeedsUpdate = "WARD_PRE_OP_NEEDS_UPDATE";

    private const string SendAction = "WardPreOpSend";
    private const string ConfirmAction = "WardPreOpConfirm";
    private const string NotApplicable = "NotApplicable";

    private static readonly string[] LateralityCodes = ["Left", "Right", "Bilateral", NotApplicable];

    private static readonly OprCaseStatus[] LockedCaseStatuses =
        [OprCaseStatus.InProgress, OprCaseStatus.Completed, OprCaseStatus.Rejected, OprCaseStatus.Cancelled];

    private static readonly PatientVitalSignStatus[] UsableVitalStatuses =
        [PatientVitalSignStatus.Recorded, PatientVitalSignStatus.Verified, PatientVitalSignStatus.Corrected];

    private static readonly JsonSerializerOptions SnapshotJson = new(JsonSerializerDefaults.Web);

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;
    private readonly OperatingRoomPreparationService _preparationService;

    public OprWardPreOpService(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor,
        LoggerService loggerService, OperatingRoomPreparationService preparationService)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
        _preparationService = preparationService;
    }

    // ---------------------------------------------------------------------
    // Baca
    // ---------------------------------------------------------------------

    /// <summary>
    /// Versi terbaru beserta butir dan penandaan; <c>null</c> bila kasus tidak ada. Kasus tanpa versi
    /// mendapat templat berisi butir aktif master (<c>IsTemplate = true</c>).
    /// </summary>
    public async Task<WardPreOpResponse?> GetLatestAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var opCase = await _dbContext.OprCases.AsNoTracking()
            .Where(x => x.Id == caseId && !x.IsDelete)
            .Select(x => new { x.Id, x.Laterality })
            .FirstOrDefaultAsync(cancellationToken);
        if (opCase == null) return null;

        var note = await _dbContext.OprWardPreOpNotes.AsNoTracking()
            .Include(x => x.Items.Where(i => !i.IsDelete)).ThenInclude(i => i.PreparationItem)
            .Include(x => x.SiteMarks.Where(m => !m.IsDelete))
            .Where(x => x.OprCaseId == caseId && !x.IsDelete)
            .OrderByDescending(x => x.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        return note == null
            ? await BuildTemplateAsync(caseId, opCase.Laterality, cancellationToken)
            : await MapAsync(note, opCase.Laterality, cancellationToken);
    }

    /// <summary>Seluruh versi, terbaru lebih dulu; <c>null</c> bila kasus tidak ada.</summary>
    public async Task<List<WardPreOpVersionSummary>?> GetVersionsAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.OprCases.AsNoTracking().AnyAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);
        if (!exists) return null;

        var notes = await _dbContext.OprWardPreOpNotes.AsNoTracking()
            .Where(x => x.OprCaseId == caseId && !x.IsDelete)
            .OrderByDescending(x => x.VersionNumber)
            .ToListAsync(cancellationToken);

        var names = await ResolveNamesAsync(
            notes.SelectMany(x => new[] { x.SentByUserId, x.ConfirmedByUserId }), cancellationToken);

        return [.. notes.Select(x => new WardPreOpVersionSummary
        {
            Id = x.Id, VersionNumber = x.VersionNumber, Status = x.Status,
            SentByName = NameOf(names, x.SentByUserId), SentAt = x.SentAt,
            ConfirmedByName = NameOf(names, x.ConfirmedByUserId), ConfirmedAt = x.ConfirmedAt,
            NeedsUpdateAt = x.NeedsUpdateAt, PreviousVersionId = x.PreviousVersionId
        })];
    }

    // ---------------------------------------------------------------------
    // Tulis
    // ---------------------------------------------------------------------

    /// <summary>
    /// Simpan draf pengirim. Bila belum ada versi, atau versi terbaru <c>NeedsUpdate</c>, versi baru
    /// dibuat dari butir aktif master dengan isian versi sebelumnya sebagai usulan.
    /// </summary>
    public async Task<WardPreOpResponse> SaveDraftAsync(Guid caseId, SaveWardPreOpDraftRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentException("Isian Catatan Pra-Operasi wajib diisi.");
        var actorUserId = GetUserId(_httpContextAccessor);

        var opCase = await _dbContext.OprCases.FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");
        EnsureCaseWritable(opCase.Status);

        var markingLaterality = NormalizeLaterality(request.MarkingLaterality);
        ValidateSiteMarks(request.SiteMarks);
        if (request.Items.Select(x => x.PreparationItemId).Distinct().Count() != request.Items.Count)
            throw new ArgumentException("Butir persiapan tidak boleh ganda.");
        // VAL-RWF-79: minimal satu titik bila sisi bukan NotApplicable.
        if (markingLaterality != null && markingLaterality != NotApplicable && request.SiteMarks.Count == 0)
            throw new ArgumentException("Tandai area operasi pada gambar tubuh");
        EnsureLateralityMatches(opCase.Laterality, markingLaterality);

        var latest = await LoadLatestTrackedAsync(caseId, cancellationToken);
        var now = DateTime.UtcNow;
        OprWardPreOpNote target;

        if (latest == null || latest.Status is OprWardPreOpStatus.NeedsUpdate or OprWardPreOpStatus.Superseded)
        {
            EnsureVersion(latest?.Version ?? 0, request.ExpectedVersion);
            target = await CreateVersionAsync(opCase.Id, latest, actorUserId, now, cancellationToken);
        }
        else if (latest.Status == OprWardPreOpStatus.Draft)
        {
            EnsureVersion(latest.Version, request.ExpectedVersion);
            target = latest;
        }
        else
        {
            throw new OperatingRoomConflictException("InvalidStateTransition",
                "Catatan pra-operasi sudah dikirim. Perubahan hanya dapat dilakukan setelah kasus ditunda.");
        }

        foreach (var itemRequest in request.Items)
        {
            var item = target.Items.FirstOrDefault(x => !x.IsDelete && x.PreparationItemId == itemRequest.PreparationItemId)
                ?? throw new ArgumentException("Butir persiapan tidak termasuk versi Catatan Pra-Operasi ini.");
            item.SenderConfirmed = itemRequest.SenderConfirmed;
            item.Note = Truncate(Normalize(itemRequest.Note), 500);
            if (_dbContext.Entry(item).State != EntityState.Added)
            {
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }
        }

        // Penandaan diganti utuh: titik lama ditandai terhapus, titik baru ditambahkan.
        foreach (var mark in target.SiteMarks.Where(x => !x.IsDelete).ToList())
        {
            if (_dbContext.Entry(mark).State == EntityState.Added)
            {
                target.SiteMarks.Remove(mark);
                _dbContext.Entry(mark).State = EntityState.Detached;
                continue;
            }
            mark.IsDelete = true;
            mark.DeleteDateTime = now;
            mark.DeleteBy = actorUserId;
        }
        foreach (var markRequest in request.SiteMarks)
        {
            var mark = new OprWardPreOpSiteMark
            {
                NoteId = target.Id, BodyView = markRequest.BodyView, X = Math.Round(markRequest.X, 2),
                Y = Math.Round(markRequest.Y, 2), Label = Truncate(Normalize(markRequest.Label), 100),
                CreateDateTime = now, CreateBy = actorUserId
            };
            _dbContext.OprWardPreOpSiteMarks.Add(mark);
        }

        target.MarkingLaterality = markingLaterality;
        target.MarkingLocationNote = Truncate(Normalize(request.MarkingLocationNote), 500);
        target.Version++;
        target.UpdateDateTime = now;
        target.UpdateBy = actorUserId;

        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "OperatingRoomWardPreOp.SaveDraft",
            "Menyimpan draf Catatan Pra-Operasi bangsal.",
            new { CaseId = caseId, NoteId = target.Id, target.VersionNumber, ActorUserId = actorUserId });
        return (await GetLatestAsync(caseId, cancellationToken))!;
    }

    /// <summary>
    /// Kirim versi draf: butir wajib pengirim lengkap, sisi penandaan cocok, dan tanda vital tersedia.
    /// Potret tanda vital dan nyeri dibekukan pada saat ini.
    /// </summary>
    public async Task<WardPreOpResponse> SendAsync(Guid caseId, SendWardPreOpRequest request, string? headerIdempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var key = ResolveIdempotencyKey(headerIdempotencyKey, request?.IdempotencyKey);
        var expectedVersion = request?.ExpectedVersion ?? 0;
        var actorUserId = GetUserId(_httpContextAccessor);
        var fingerprint = Hash($"{SendAction}|{expectedVersion}");

        var prior = await FindIdempotentAsync(SendAction, key, cancellationToken);
        if (prior != null)
        {
            EnsureSameCase(prior, caseId);
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetLatestAsync(caseId, cancellationToken))!;
        }

        var opCase = await _dbContext.OprCases.FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");
        EnsureCaseWritable(opCase.Status);

        var latest = await LoadLatestTrackedAsync(caseId, cancellationToken);
        if (latest == null || latest.Status != OprWardPreOpStatus.Draft)
            throw new OperatingRoomConflictException("InvalidStateTransition",
                "Tidak ada draf Catatan Pra-Operasi yang dapat dikirim.");
        EnsureVersion(latest.Version, expectedVersion);

        // VAL-RWF-76
        var missing = latest.Items.Where(x => !x.IsDelete && x.IsMandatorySnapshot && !x.SenderConfirmed)
            .Select(x => x.ItemNameSnapshot).ToList();
        if (missing.Count > 0)
            throw new OperatingRoomUnprocessableException(MandatoryItemsIncompleteCode,
                $"Butir wajib belum lengkap: {string.Join(", ", missing)}");

        if (latest.MarkingLaterality == null)
            throw new ArgumentException("Pilih sisi penandaan area operasi sebelum mengirim.");
        if (latest.MarkingLaterality != NotApplicable && !latest.SiteMarks.Any(x => !x.IsDelete))
            throw new ArgumentException("Tandai area operasi pada gambar tubuh");
        EnsureLateralityMatches(opCase.Laterality, latest.MarkingLaterality);

        // VAL-RWF-78: tanda vital terakhir kunjungan kasus.
        var vital = await _dbContext.Set<TrxPatientVitalSign>().AsNoTracking()
            .Where(x => x.EncounterId == opCase.EncounterId && x.PatientId == opCase.PatientId && !x.IsDelete && !x.IsCancel &&
                UsableVitalStatuses.Contains(x.VitalSignStatus))
            .OrderByDescending(x => x.ObservationDateTime)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new OperatingRoomUnprocessableException(VitalSignMissingCode, "Catat tanda vital pasien lebih dulu");

        var pain = await ReadPainSnapshotAsync(opCase.EncounterId, opCase.PatientId, vital, cancellationToken);
        var now = DateTime.UtcNow;

        latest.VitalSnapshotJson = JsonSerializer.Serialize(new WardPreOpVitalSnapshot
        {
            VitalSignId = vital.Id, SystolicBp = vital.BloodPressureSystolic, DiastolicBp = vital.BloodPressureDiastolic,
            PulseRate = vital.PulseRate, RespiratoryRate = vital.RespiratoryRate, Temperature = vital.Temperature,
            SpO2 = vital.OxygenSaturation, RecordedAt = vital.ObservationDateTime
        }, SnapshotJson);
        latest.PainSnapshotJson = pain == null ? null : JsonSerializer.Serialize(pain, SnapshotJson);
        latest.Status = OprWardPreOpStatus.Sent;
        latest.SentByUserId = actorUserId;
        latest.SentAt = now;
        latest.Version++;
        latest.UpdateDateTime = now;
        latest.UpdateBy = actorUserId;

        // State matrix 9.2: versi baru terkirim → versi lama yang "perlu diperbarui" digantikan.
        var older = await _dbContext.OprWardPreOpNotes
            .Where(x => x.OprCaseId == caseId && !x.IsDelete && x.Id != latest.Id && x.Status == OprWardPreOpStatus.NeedsUpdate)
            .ToListAsync(cancellationToken);
        foreach (var note in older) MarkStatus(note, OprWardPreOpStatus.Superseded, actorUserId, now);

        _dbContext.OprStatusHistories.Add(NewHistory(caseId, opCase.Status, opCase.Status, SendAction,
            $"v{latest.VersionNumber}", key, fingerprint, actorUserId, now));
        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "OperatingRoomWardPreOp.Send",
            "Mengirim Catatan Pra-Operasi bangsal.",
            new { CaseId = caseId, NoteId = latest.Id, latest.VersionNumber, ActorUserId = actorUserId, CorrelationId = key });
        return (await GetLatestAsync(caseId, cancellationToken))!;
    }

    /// <summary>
    /// Konfirmasi penerima per butir dan penandaan, dari akun yang berbeda dengan pengirim. Versi
    /// menjadi <c>Confirmed</c> bila seluruh butir wajib dan penandaan sudah dikonfirmasi; bila
    /// prasyarat lain sudah terpenuhi, kasus langsung naik ke <c>Ready</c> dalam transaksi yang sama.
    /// </summary>
    public async Task<WardPreOpResponse> ConfirmAsync(Guid caseId, ConfirmWardPreOpRequest request,
        string? headerIdempotencyKey, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentException("Isian konfirmasi wajib diisi.");
        var key = ResolveIdempotencyKey(headerIdempotencyKey, request.IdempotencyKey);
        var actorUserId = GetUserId(_httpContextAccessor);
        var fingerprint = Hash(string.Join('|', ConfirmAction, request.ExpectedVersion, request.SiteMarkingConfirmed,
            string.Join(',', request.Items.OrderBy(x => x.ItemId)
                .Select(x => $"{x.ItemId:N}:{x.ReceiverConfirmed}:{Normalize(x.Note)}"))));

        var prior = await FindIdempotentAsync(ConfirmAction, key, cancellationToken);
        if (prior != null)
        {
            EnsureSameCase(prior, caseId);
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetLatestAsync(caseId, cancellationToken))!;
        }

        var opCase = await _dbContext.OprCases.FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");
        EnsureCaseWritable(opCase.Status);

        var latest = await LoadLatestTrackedAsync(caseId, cancellationToken);
        if (latest == null || latest.Status != OprWardPreOpStatus.Sent)
            throw new OperatingRoomConflictException("InvalidStateTransition",
                "Catatan pra-operasi belum dikirim atau sudah dikonfirmasi.");
        EnsureVersion(latest.Version, request.ExpectedVersion);

        // VAL-RWF-75
        if (latest.SentByUserId == actorUserId)
            throw new OperatingRoomUnprocessableException(SameAccountCode, "Pengirim dan penerima harus akun yang berbeda");
        EnsureLateralityMatches(opCase.Laterality, latest.MarkingLaterality);

        if (request.Items.Select(x => x.ItemId).Distinct().Count() != request.Items.Count)
            throw new ArgumentException("Butir konfirmasi tidak boleh ganda.");

        var now = DateTime.UtcNow;
        foreach (var itemRequest in request.Items)
        {
            var item = latest.Items.FirstOrDefault(x => !x.IsDelete && x.Id == itemRequest.ItemId)
                ?? throw new ArgumentException("Butir konfirmasi tidak termasuk versi Catatan Pra-Operasi ini.");
            item.ReceiverConfirmed = itemRequest.ReceiverConfirmed;
            item.ReceiverConfirmedByUserId = itemRequest.ReceiverConfirmed ? actorUserId : null;
            item.ReceiverConfirmedAt = itemRequest.ReceiverConfirmed ? now : null;
            var receiverNote = Normalize(itemRequest.Note);
            // Kolom catatan butir milik pengirim; catatan penerima ditambahkan, tidak menimpa.
            if (receiverNote != null)
                item.Note = Truncate(item.Note == null ? $"Penerima: {receiverNote}" : $"{item.Note} | Penerima: {receiverNote}", 500);
            item.UpdateDateTime = now;
            item.UpdateBy = actorUserId;
        }

        latest.SiteMarkingConfirmed = request.SiteMarkingConfirmed;
        var markingRequired = latest.MarkingLaterality != NotApplicable || latest.SiteMarks.Any(x => !x.IsDelete);
        var allMandatoryConfirmed = latest.Items.Where(x => !x.IsDelete && x.IsMandatorySnapshot).All(x => x.ReceiverConfirmed);
        if (allMandatoryConfirmed && (!markingRequired || latest.SiteMarkingConfirmed))
        {
            latest.Status = OprWardPreOpStatus.Confirmed;
            latest.ConfirmedByUserId = actorUserId;
            latest.ConfirmedAt = now;
        }
        latest.Version++;
        latest.UpdateDateTime = now;
        latest.UpdateBy = actorUserId;

        _dbContext.OprStatusHistories.Add(NewHistory(caseId, opCase.Status, opCase.Status, ConfirmAction,
            $"v{latest.VersionNumber}:{latest.Status}", key, fingerprint, actorUserId, now));

        var becameReady = latest.Status == OprWardPreOpStatus.Confirmed &&
            await _preparationService.TryCompleteReadinessAsync(caseId, actorUserId, now, key, cancellationToken);
        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "OperatingRoomWardPreOp.Confirm",
            "Mengonfirmasi Catatan Pra-Operasi bangsal.",
            new
            {
                CaseId = caseId, NoteId = latest.Id, latest.VersionNumber, Status = latest.Status.ToString(),
                CaseBecameReady = becameReady, ActorUserId = actorUserId, CorrelationId = key
            });
        return (await GetLatestAsync(caseId, cancellationToken))!;
    }

    // ---------------------------------------------------------------------
    // Dipanggil service OK lain dalam transaksi mereka (tanpa SaveChanges)
    // ---------------------------------------------------------------------

    /// <summary>
    /// Penundaan kasus: versi <c>Sent</c>/<c>Confirmed</c> menjadi <c>NeedsUpdate</c> (<c>RWI-DEC-199</c>).
    /// Pemanggil menyimpan dalam transaksi penundaannya.
    /// </summary>
    internal static async Task MarkNeedsUpdateAsync(ApplicationDbContext db, Guid caseId, Guid actorUserId, DateTime now,
        CancellationToken cancellationToken)
    {
        var notes = await db.OprWardPreOpNotes
            .Where(x => x.OprCaseId == caseId && !x.IsDelete &&
                (x.Status == OprWardPreOpStatus.Sent || x.Status == OprWardPreOpStatus.Confirmed))
            .ToListAsync(cancellationToken);
        foreach (var note in notes)
        {
            MarkStatus(note, OprWardPreOpStatus.NeedsUpdate, actorUserId, now);
            note.NeedsUpdateAt = now;
        }
    }

    /// <summary>Kasus ditolak atau dibatalkan: seluruh versi yang belum <c>Superseded</c> digantikan.</summary>
    internal static async Task SupersedeAllAsync(ApplicationDbContext db, Guid caseId, Guid actorUserId, DateTime now,
        CancellationToken cancellationToken)
    {
        var notes = await db.OprWardPreOpNotes
            .Where(x => x.OprCaseId == caseId && !x.IsDelete && x.Status != OprWardPreOpStatus.Superseded)
            .ToListAsync(cancellationToken);
        foreach (var note in notes) MarkStatus(note, OprWardPreOpStatus.Superseded, actorUserId, now);
    }

    /// <summary>
    /// Syarat keempat gerbang "Siap" (<c>INV-RWF-26</c>, <c>INV-RWF-27</c>, <c>VAL-RWF-80</c>). Membaca
    /// juga perubahan yang belum tersimpan pada konteks yang sama, supaya konfirmasi terakhir langsung
    /// dapat menutup gerbang dalam satu perintah.
    /// </summary>
    internal static async Task<WardPreOpGate> ReadGateAsync(ApplicationDbContext db, Guid caseId, string? caseLaterality,
        CancellationToken cancellationToken)
    {
        var saved = await db.OprWardPreOpNotes.AsNoTracking()
            .Where(x => x.OprCaseId == caseId && !x.IsDelete)
            .Select(x => new GateRow(x.Id, x.VersionNumber, x.Status, x.MarkingLaterality))
            .ToListAsync(cancellationToken);
        var pending = db.OprWardPreOpNotes.Local
            .Where(x => x.OprCaseId == caseId && !x.IsDelete)
            .Select(x => new GateRow(x.Id, x.VersionNumber, x.Status, x.MarkingLaterality))
            .ToList();
        var latest = pending
            .Concat(saved.Where(s => pending.All(p => p.Id != s.Id)))
            .OrderByDescending(x => x.VersionNumber)
            .FirstOrDefault();

        if (latest == null || latest.Status is OprWardPreOpStatus.Draft or OprWardPreOpStatus.Sent or OprWardPreOpStatus.Superseded)
            return new WardPreOpGate(BlockerIncomplete, "Catatan pra-operasi belum dikonfirmasi");
        if (latest.Status == OprWardPreOpStatus.NeedsUpdate)
            return new WardPreOpGate(BlockerNeedsUpdate, "Catatan pra-operasi perlu diperbarui setelah penundaan");
        if (IsLateralityMismatch(caseLaterality, latest.MarkingLaterality))
            return new WardPreOpGate(BlockerIncomplete, "Sisi penandaan berbeda dengan sisi pada pesanan operasi");
        return WardPreOpGate.Satisfied;
    }

    // ---------------------------------------------------------------------
    // Pembantu
    // ---------------------------------------------------------------------

    private async Task<OprWardPreOpNote> CreateVersionAsync(Guid caseId, OprWardPreOpNote? previous, Guid actorUserId,
        DateTime now, CancellationToken cancellationToken)
    {
        // Butir nonaktif tidak muncul di versi baru; versi lama tetap utuh.
        var activeItems = await _dbContext.Set<MstSurgicalPreparationItem>().AsNoTracking()
            .Where(x => !x.IsDelete && !x.IsCancel && x.IsActive)
            .ToListAsync(cancellationToken);
        if (activeItems.Count == 0)
            throw new OperatingRoomUnprocessableException("PreparationItemsNotConfigured",
                "Master butir persiapan bedah belum berisi butir aktif. Hubungi admin Master Data.");

        var version = new OprWardPreOpNote
        {
            OprCaseId = caseId,
            VersionNumber = (previous?.VersionNumber ?? 0) + 1,
            PreviousVersionId = previous?.Id,
            Status = OprWardPreOpStatus.Draft,
            MarkingLaterality = previous?.MarkingLaterality,
            MarkingLocationNote = previous?.MarkingLocationNote,
            Version = 0,
            CreateDateTime = now,
            CreateBy = actorUserId
        };

        foreach (var master in activeItems
                     .OrderBy(x => SurgicalPreparationItemService.GroupRank(x.GroupName))
                     .ThenBy(x => x.SortOrder).ThenBy(x => x.ItemName))
        {
            // Isian versi lama dipakai sebagai usulan; penerima selalu mulai dari belum dikonfirmasi.
            var old = previous?.Items.FirstOrDefault(x => !x.IsDelete && x.PreparationItemId == master.Id);
            version.Items.Add(new OprWardPreOpItem
            {
                NoteId = version.Id, PreparationItemId = master.Id, ItemNameSnapshot = master.ItemName,
                IsMandatorySnapshot = master.IsMandatory, SenderConfirmed = old?.SenderConfirmed ?? false,
                Note = old?.Note, CreateDateTime = now, CreateBy = actorUserId
            });
        }

        if (previous != null)
        {
            foreach (var mark in previous.SiteMarks.Where(x => !x.IsDelete))
                version.SiteMarks.Add(new OprWardPreOpSiteMark
                {
                    NoteId = version.Id, BodyView = mark.BodyView, X = mark.X, Y = mark.Y, Label = mark.Label,
                    CreateDateTime = now, CreateBy = actorUserId
                });
        }

        _dbContext.OprWardPreOpNotes.Add(version);
        return version;
    }

    private Task<OprWardPreOpNote?> LoadLatestTrackedAsync(Guid caseId, CancellationToken cancellationToken) =>
        _dbContext.OprWardPreOpNotes
            .Include(x => x.Items)
            .Include(x => x.SiteMarks)
            .Where(x => x.OprCaseId == caseId && !x.IsDelete)
            .OrderByDescending(x => x.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Nyeri terakhir dari pengkajian nyeri yang selesai; bila belum ada, dari skala nyeri pada tanda
    /// vital yang sama. Kosong bila belum pernah dinilai.
    /// </summary>
    private async Task<WardPreOpPainSnapshot?> ReadPainSnapshotAsync(Guid encounterId, Guid patientId,
        TrxPatientVitalSign vital, CancellationToken cancellationToken)
    {
        var assessment = await _dbContext.Set<TrxPatientAssessment>().AsNoTracking()
            .Where(x => x.EncounterId == encounterId && x.PatientId == patientId && !x.IsDelete && !x.IsCancel &&
                x.AssessmentType == PatientAssessmentType.PainMonitoring &&
                x.AssessmentStatus == PatientAssessmentStatus.Completed &&
                x.PainAssessmentState != PainAssessmentState.NotAssessed)
            .OrderByDescending(x => x.AssessmentDateTime)
            .Select(x => new { x.Id, x.PainScale, x.PainAssessmentState, x.AssessmentDateTime })
            .FirstOrDefaultAsync(cancellationToken);

        if (assessment != null)
        {
            var scaleName = await (
                    from response in _dbContext.Set<CliAssessmentInstrumentResponse>().AsNoTracking()
                    join version in _dbContext.Set<CliClinicalInstrumentVersion>().AsNoTracking()
                        on response.InstrumentVersionId equals version.Id
                    join instrument in _dbContext.Set<CliClinicalInstrument>().AsNoTracking()
                        on version.InstrumentId equals instrument.Id
                    where response.AssessmentId == assessment.Id && !response.IsDelete
                    select instrument.Name)
                .FirstOrDefaultAsync(cancellationToken);

            return new WardPreOpPainSnapshot
            {
                Score = assessment.PainAssessmentState == PainAssessmentState.NoPain ? 0 : assessment.PainScale,
                ScaleName = scaleName ?? "Skala nyeri",
                RecordedAt = assessment.AssessmentDateTime,
                SourceId = assessment.Id,
                SourceKind = "PainAssessment"
            };
        }

        if (vital.HasPain || vital.PainScale.HasValue)
        {
            return new WardPreOpPainSnapshot
            {
                Score = vital.PainScale ?? 0,
                ScaleName = "Skala nyeri pada tanda vital",
                RecordedAt = vital.ObservationDateTime,
                SourceId = vital.Id,
                SourceKind = "VitalSign"
            };
        }

        return null;
    }

    private async Task<WardPreOpResponse> BuildTemplateAsync(Guid caseId, string? caseLaterality,
        CancellationToken cancellationToken)
    {
        var items = await _dbContext.Set<MstSurgicalPreparationItem>().AsNoTracking()
            .Where(x => !x.IsDelete && !x.IsCancel && x.IsActive)
            .ToListAsync(cancellationToken);

        return new WardPreOpResponse
        {
            Id = Guid.Empty, OprCaseId = caseId, IsTemplate = true, VersionNumber = 0, Status = null,
            CaseLaterality = caseLaterality, Version = 0,
            Items = [.. items
                .OrderBy(x => SurgicalPreparationItemService.GroupRank(x.GroupName))
                .ThenBy(x => x.SortOrder).ThenBy(x => x.ItemName)
                .Select(x => new WardPreOpItemResponse
                {
                    ItemId = Guid.Empty, PreparationItemId = x.Id, GroupName = x.GroupName, ItemName = x.ItemName,
                    IsMandatory = x.IsMandatory
                })]
        };
    }

    private async Task<WardPreOpResponse> MapAsync(OprWardPreOpNote note, string? caseLaterality,
        CancellationToken cancellationToken)
    {
        var names = await ResolveNamesAsync(
            new[] { note.SentByUserId, note.ConfirmedByUserId }
                .Concat(note.Items.Select(x => x.ReceiverConfirmedByUserId)), cancellationToken);

        return new WardPreOpResponse
        {
            Id = note.Id, OprCaseId = note.OprCaseId, IsTemplate = false, VersionNumber = note.VersionNumber,
            Status = note.Status,
            VitalSnapshot = Deserialize<WardPreOpVitalSnapshot>(note.VitalSnapshotJson),
            PainSnapshot = Deserialize<WardPreOpPainSnapshot>(note.PainSnapshotJson),
            Items = [.. note.Items.Where(x => !x.IsDelete)
                .OrderBy(x => SurgicalPreparationItemService.GroupRank(x.PreparationItem?.GroupName))
                .ThenBy(x => x.PreparationItem?.SortOrder ?? int.MaxValue)
                .ThenBy(x => x.ItemNameSnapshot)
                .Select(x => new WardPreOpItemResponse
                {
                    ItemId = x.Id, PreparationItemId = x.PreparationItemId,
                    GroupName = x.PreparationItem?.GroupName ?? string.Empty, ItemName = x.ItemNameSnapshot,
                    IsMandatory = x.IsMandatorySnapshot, SenderConfirmed = x.SenderConfirmed,
                    ReceiverConfirmed = x.ReceiverConfirmed,
                    ReceiverConfirmedByName = NameOf(names, x.ReceiverConfirmedByUserId),
                    ReceiverConfirmedAt = x.ReceiverConfirmedAt, Note = x.Note
                })],
            SiteMarks = [.. note.SiteMarks.Where(x => !x.IsDelete).Select(x => new WardPreOpSiteMarkResponse
            {
                Id = x.Id, BodyView = x.BodyView, X = x.X, Y = x.Y, Label = x.Label
            })],
            MarkingLaterality = note.MarkingLaterality, MarkingLocationNote = note.MarkingLocationNote,
            SiteMarkingConfirmed = note.SiteMarkingConfirmed, CaseLaterality = caseLaterality,
            SentByName = NameOf(names, note.SentByUserId), SentAt = note.SentAt,
            ConfirmedByName = NameOf(names, note.ConfirmedByUserId), ConfirmedAt = note.ConfirmedAt,
            NeedsUpdateAt = note.NeedsUpdateAt, PreviousVersionId = note.PreviousVersionId, Version = note.Version
        };
    }

    private async Task<Dictionary<Guid, string>> ResolveNamesAsync(IEnumerable<Guid?> userIds,
        CancellationToken cancellationToken)
    {
        var ids = userIds.Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<Guid, string>();
        return await _dbContext.Users.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);
    }

    private static string? NameOf(Dictionary<Guid, string> names, Guid? userId) =>
        userId.HasValue && names.TryGetValue(userId.Value, out var name) ? name : null;

    private static T? Deserialize<T>(string? json) where T : class
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<T>(json, SnapshotJson); }
        catch (JsonException) { return null; }
    }

    private static void MarkStatus(OprWardPreOpNote note, OprWardPreOpStatus status, Guid actorUserId, DateTime now)
    {
        note.Status = status;
        note.Version++;
        note.UpdateDateTime = now;
        note.UpdateBy = actorUserId;
    }

    private static void EnsureCaseWritable(OprCaseStatus status)
    {
        // VAL-RWF-77
        if (LockedCaseStatuses.Contains(status))
            throw new OperatingRoomUnprocessableException(CaseLockedCode, "Catatan pra-operasi tidak dapat diubah pada kasus ini");
    }

    /// <summary>VAL-RWF-74: dibandingkan hanya bila sisi kasus salah satu dari empat kode dan bukan NotApplicable.</summary>
    private static void EnsureLateralityMatches(string? caseLaterality, string? markingLaterality)
    {
        if (IsLateralityMismatch(caseLaterality, markingLaterality))
            throw new OperatingRoomUnprocessableException(LateralityMismatchCode,
                "Sisi penandaan berbeda dengan sisi pada pesanan operasi");
    }

    private static bool IsLateralityMismatch(string? caseLaterality, string? markingLaterality)
    {
        var caseCode = LateralityCodes.FirstOrDefault(x => string.Equals(x, caseLaterality?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (caseCode == null || caseCode == NotApplicable || markingLaterality == null) return false;
        return !string.Equals(caseCode, markingLaterality, StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeLaterality(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return LateralityCodes.FirstOrDefault(x => string.Equals(x, value.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException("Sisi penandaan harus Left, Right, Bilateral, atau NotApplicable.");
    }

    private static void ValidateSiteMarks(IReadOnlyCollection<WardPreOpSiteMarkRequest> marks)
    {
        foreach (var mark in marks)
        {
            if (!Enum.IsDefined(mark.BodyView))
                throw new ArgumentException("Sisi gambar tubuh tidak dikenal.");
            if (mark.X is < 0 or > 100 || mark.Y is < 0 or > 100)
                throw new ArgumentException("Titik penandaan harus berada di dalam gambar (0–100).");
        }
    }

    private static string ResolveIdempotencyKey(string? header, string? body)
    {
        var key = Normalize(header) ?? Normalize(body)
            ?? throw new ArgumentException("Header Idempotency-Key wajib diisi.");
        if (key.Length > 100) throw new ArgumentException("Idempotency key maksimal 100 karakter.");
        return key;
    }

    private static string? Truncate(string? value, int max) =>
        value == null || value.Length <= max ? value : value[..max];

    private Task<OprStatusHistory?> FindIdempotentAsync(string action, string key, CancellationToken cancellationToken) =>
        _dbContext.OprStatusHistories.AsNoTracking().FirstOrDefaultAsync(x =>
            x.Action == action && x.CorrelationId == key && !x.IsDelete, cancellationToken);

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            _dbContext.ChangeTracker.Clear();
            throw new OperatingRoomConflictException("OPR012",
                "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
        }
    }

    private sealed record GateRow(Guid Id, int VersionNumber, OprWardPreOpStatus Status, string? MarkingLaterality);
}

/// <summary>Hasil gerbang pra-operasi bangsal; <c>Blocker</c> kosong berarti terpenuhi.</summary>
internal sealed record WardPreOpGate(string? Blocker, string? Message)
{
    public static readonly WardPreOpGate Satisfied = new(null, null);
}
