using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using static QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services.OperatingRoomCommandSupport;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;

public sealed class OperatingRoomCaseService
{

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;

    private readonly OperatingRoomRuleRelaxation _relaxation;

    public OperatingRoomCaseService(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor,
        LoggerService loggerService, OperatingRoomRuleRelaxation relaxation)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
        _relaxation = relaxation;
    }

    public async Task<PagedResult<OprCaseSummaryResponse>> GetPagedAsync(OprCasePagedQuery request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.OprCases.AsNoTracking().Where(x => !x.IsDelete);
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status);
        if (request.PatientId.HasValue) query = query.Where(x => x.PatientId == request.PatientId);
        if (request.EncounterId.HasValue) query = query.Where(x => x.EncounterId == request.EncounterId);
        if (request.RequestedFrom.HasValue) query = query.Where(x => x.RequestedAt >= request.RequestedFrom.Value);
        if (request.RequestedTo.HasValue) query = query.Where(x => x.RequestedAt <= request.RequestedTo.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x => x.CaseNumber.ToLower().Contains(search) ||
                (x.Patient != null && x.Patient.FullName.ToLower().Contains(search)));
        }

        var totalData = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.RequestedAt).ThenByDescending(x => x.Id)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => new OprCaseSummaryResponse
            {
                Id = x.Id, CaseNumber = x.CaseNumber, PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                EncounterId = x.EncounterId, CaseType = x.CaseType, Priority = x.Priority,
                Status = x.Status, RequestedAt = x.RequestedAt, Version = x.Version,
                PrimaryProcedureName = x.Procedures.Where(p => p.IsPrimary && !p.IsDelete)
                    .Select(p => p.PatientProcedure != null ? p.PatientProcedure.ProcedureNameSnapshot : string.Empty)
                    .FirstOrDefault() ?? string.Empty,
                // BE-RWI-174: isian tambahan API 11.5.1.
                SurgicalServiceType = x.SurgicalServiceType,
                PlannedAnesthesiaType = x.PlannedAnesthesiaType,
                RejectedAt = x.RejectedAt,
                RejectionReason = x.RejectionReason,
                RejectedByName = x.RejectedByUserId == null
                    ? null
                    : _dbContext.Users.Where(u => u.Id == x.RejectedByUserId).Select(u => u.DisplayName).FirstOrDefault(),
                LastStatusReason = x.StatusHistories
                    .Where(h => !h.IsDelete && StatusReasonActions.Contains(h.Action))
                    .OrderByDescending(h => h.OccurredAt)
                    .Select(h => h.Reason)
                    .FirstOrDefault(),
                WardPreOpStatus = _dbContext.OprWardPreOpNotes
                    .Where(n => n.OprCaseId == x.Id && !n.IsDelete)
                    .OrderByDescending(n => n.VersionNumber)
                    .Select(n => (OprWardPreOpStatus?)n.Status)
                    .FirstOrDefault(),
                HandoverStatus = _dbContext.OprHandovers
                    .Where(h => h.OprCaseId == x.Id && !h.IsDelete)
                    .OrderByDescending(h => h.Revision)
                    .Select(h => (OprHandoverStatus?)h.Status)
                    .FirstOrDefault()
            }).ToListAsync(cancellationToken);

        return new PagedResult<OprCaseSummaryResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = totalData,
            TotalPage = (int)Math.Ceiling(totalData / (double)request.PageSize), Items = items
        };
    }

    public async Task<OprCaseDetailResponse?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await LoadCaseAsync(id, false, cancellationToken);
        if (entity == null) return null;

        var detail = MapDetail(entity);

        // BE-RWI-174: nama penolak, status pra-operasi bangsal, dan status serah terima terakhir.
        if (entity.RejectedByUserId.HasValue)
            detail.RejectedByName = await _dbContext.Users.AsNoTracking()
                .Where(u => u.Id == entity.RejectedByUserId.Value)
                .Select(u => u.DisplayName)
                .FirstOrDefaultAsync(cancellationToken);
        detail.WardPreOpStatus = await _dbContext.OprWardPreOpNotes.AsNoTracking()
            .Where(n => n.OprCaseId == id && !n.IsDelete)
            .OrderByDescending(n => n.VersionNumber)
            .Select(n => (OprWardPreOpStatus?)n.Status)
            .FirstOrDefaultAsync(cancellationToken);
        detail.HandoverStatus = await _dbContext.OprHandovers.AsNoTracking()
            .Where(h => h.OprCaseId == id && !h.IsDelete)
            .OrderByDescending(h => h.Revision)
            .Select(h => (OprHandoverStatus?)h.Status)
            .FirstOrDefaultAsync(cancellationToken);
        return detail;
    }

    public Task<OprCaseDetailResponse> CreateAsync(CreateOprCaseRequest request, CancellationToken cancellationToken = default) =>
        CreateCoreAsync(request, enforceDoctorActor: !_relaxation.IsRelaxed, requestNote: null, cancellationToken);

    /// <summary>
    /// Jalur pembuatan kasus khusus pemesanan ruang bedah dari bangsal (<c>BE-RWI-175</c>,
    /// <c>InpSurgeryBookingAdapter</c>). Bedanya dengan <see cref="CreateAsync"/> hanya satu: akun
    /// login tidak wajib dokter pemohon, karena perawat bangsal boleh menginput atas order dokter
    /// (<c>RWI-DEC-176</c>); dokter pemohon dan operator diambil adapter dari order tindakan yang
    /// dirujuk. Seluruh validasi rujukan, idempotensi, dan transaksi tetap milik service ini.
    /// </summary>
    /// <param name="request">Permintaan kasus yang disusun adapter dari order tindakan.</param>
    /// <param name="wardNote">
    /// Catatan bangsal (maks. 1000). <c>OprCase</c> tidak punya kolom catatan, sehingga catatan
    /// disimpan sebagai alasan baris histori <c>Request</c> — terbaca pada riwayat status kasus.
    /// </param>
    /// <param name="cancellationToken">Token pembatalan.</param>
    public Task<OprCaseDetailResponse> CreateFromWardBookingAsync(CreateOprCaseRequest request, string? wardNote,
        CancellationToken cancellationToken = default) =>
        CreateCoreAsync(request, enforceDoctorActor: false, requestNote: wardNote, cancellationToken);

    /// <summary>
    /// Menolak order operasi berstatus Diminta dengan alasan (<c>BE-RWI-174</c>, <c>INV-RWF-31</c>).
    /// Status <c>Rejected</c> final: kasus tidak dapat diubah, dijadwalkan, ditunda, dibatalkan,
    /// atau dimulai (<c>OPR-CASE-REJ-002</c>); bangsal memesan ulang sebagai kasus baru.
    /// </summary>
    /// <remarks>
    /// Contoh: kasus OK-2026-0142 milik Budi S. berstatus Diminta. Petugas penjadwalan menolak dengan
    /// alasan "Hasil lab pra-operasi belum ada" → status Ditolak, penolak dan waktunya tersimpan, dan
    /// order tindakan "Appendektomi" bebas dirujuk kasus baru.
    /// </remarks>
    public async Task<OprCaseDetailResponse> RejectAsync(Guid id, RejectOprCaseRequest request, string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        // VAL-RWF-82: alasan 10–500 karakter → 400 "Isi alasan penolakan".
        var reason = request.Reason?.Trim() ?? string.Empty;
        if (reason.Length < 10 || reason.Length > 500)
            throw new ArgumentException("Isi alasan penolakan (10–500 karakter).");
        var key = Normalize(idempotencyKey) ?? Normalize(request.IdempotencyKey)
            ?? throw new ArgumentException("Header Idempotency-Key wajib diisi.");
        if (key.Length > 100) throw new ArgumentException("Idempotency key maksimal 100 karakter.");

        var actorUserId = GetCurrentUserId();
        var fingerprint = Hash(reason);
        var prior = await FindIdempotentCaseAsync(RejectAction, key, cancellationToken);
        if (prior != null)
        {
            if (prior.OprCaseId != id)
                throw new OperatingRoomConflictException("OPR013", "Idempotency key sudah digunakan untuk kasus lain.");
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(id, cancellationToken))!;
        }

        var entity = await _dbContext.OprCases.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");
        // State matrix 9.1: kasus Rejected yang ditolak lagi → OPR-CASE-REJ-002; status lain → REJ-001.
        EnsureNotRejected(entity.Status);
        if (entity.Status != OprCaseStatus.Requested)
            throw new OperatingRoomUnprocessableException(RejectNotRequestedCode,
                "Hanya pesanan berstatus Diminta yang dapat ditolak.");
        if (entity.Version != request.ExpectedVersion)
            throw new OperatingRoomConflictException("OPR012", "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");

        var now = DateTime.UtcNow;
        // State matrix 9.2: seluruh versi pra-operasi yang belum Superseded ikut gugur, dalam transaksi yang sama.
        await OprWardPreOpService.SupersedeAllAsync(_dbContext, entity.Id, actorUserId, now, cancellationToken);
        entity.Status = OprCaseStatus.Rejected;
        entity.RejectedAt = now;
        entity.RejectedByUserId = actorUserId;
        entity.RejectionReason = reason;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;
        _dbContext.OprStatusHistories.Add(new OprStatusHistory
        {
            OprCaseId = entity.Id, FromStatus = OprCaseStatus.Requested, ToStatus = OprCaseStatus.Rejected,
            Action = RejectAction, Reason = reason, ActorUserId = actorUserId, OccurredAt = now,
            Source = BuildSource(fingerprint), CorrelationId = key, CreateDateTime = now, CreateBy = actorUserId
        });

        try { await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException)
        {
            _dbContext.ChangeTracker.Clear();
            var concurrent = await FindIdempotentCaseAsync(RejectAction, key, cancellationToken);
            if (concurrent != null && concurrent.OprCaseId == id)
            {
                EnsureSameFingerprint(concurrent.Source, fingerprint);
                return (await GetDetailAsync(id, cancellationToken))!;
            }
            throw new OperatingRoomConflictException("OPR012", "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
        }

        await _loggerService.AuditAsync(LogCategory, "OperatingRoomCase.Reject", "Menolak order operasi.",
            new { entity.Id, entity.CaseNumber, ActorUserId = actorUserId, Status = entity.Status.ToString(), CorrelationId = key });
        return (await GetDetailAsync(entity.Id, cancellationToken))!;
    }

    /// <summary>
    /// Penjaga <c>OPR-CASE-REJ-002</c> untuk perintah yang berasal dari service lain (jadwal, tunda,
    /// batal, mulai): kasus <c>Rejected</c> final dan tidak dapat diproses lagi.
    /// </summary>
    internal static void EnsureNotRejected(OprCaseStatus status)
    {
        if (status == OprCaseStatus.Rejected)
            throw new OperatingRoomUnprocessableException(RejectedFinalCode,
                "Kasus yang ditolak tidak dapat diubah; pesan ulang sebagai kasus baru.");
    }

    private async Task<OprCaseDetailResponse> CreateCoreAsync(CreateOprCaseRequest request, bool enforceDoctorActor,
        string? requestNote, CancellationToken cancellationToken)
    {
        ValidateRequest(request.Procedures, request.Indication, request.IdempotencyKey);
        if (request.SurgicalServiceType.HasValue && !Enum.IsDefined(request.SurgicalServiceType.Value))
            throw new ArgumentException("Jenis layanan bedah tidak dikenal.");
        if (request.PlannedAnesthesiaType.HasValue && !Enum.IsDefined(request.PlannedAnesthesiaType.Value))
            throw new ArgumentException("Rencana jenis anestesi tidak dikenal.");
        var actorUserId = GetCurrentUserId();
        if (enforceDoctorActor)
            EnsureDoctorActor(GetCurrentDoctorId(), request.RequesterDoctorId);
        // Catatan bangsal ikut sidik jari hanya bila ada, supaya kunci sama dengan catatan berbeda
        // tetap ditolak 409 (BE-RWI-175 AC 5) tanpa mengubah sidik jari permintaan OK yang sudah ada.
        var note = Normalize(requestNote);
        var fingerprint = note == null
            ? BuildFingerprint(request)
            : Hash(string.Join('|', BuildFingerprint(request), note));

        var prior = await FindIdempotentCaseAsync("Request", request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(prior.OprCaseId, cancellationToken))!;
        }

        await ValidateReferencesAsync(request.PatientId, request.EncounterId, request.RequesterDoctorId,
            request.PrimarySurgeonId, request.Procedures, null, cancellationToken);

        var now = DateTime.UtcNow;
        var entity = new OprCase
        {
            Id = CreateDeterministicId(request.IdempotencyKey),
            PatientId = request.PatientId, EncounterId = request.EncounterId,
            RequesterDoctorId = request.RequesterDoctorId, PrimarySurgeonId = request.PrimarySurgeonId,
            CaseType = request.CaseType, Priority = request.Priority, Status = OprCaseStatus.Requested,
            Indication = request.Indication.Trim(), Laterality = Normalize(request.Laterality),
            EstimatedMinutes = request.EstimatedMinutes, RequestedAt = now,
            PreferredAt = request.PreferredAt?.ToUniversalTime(), Version = 0,
            SurgicalServiceType = request.SurgicalServiceType ?? OprSurgicalServiceType.General,
            PlannedAnesthesiaType = request.PlannedAnesthesiaType,
            CreateDateTime = now, CreateBy = actorUserId
        };
        entity.CaseNumber = $"OPR-{entity.Id:N}";
        AddProcedures(entity, request.Procedures, actorUserId, now);
        var requestHistory = NewHistory(entity.Id, entity.Status, null, "Request", request.IdempotencyKey,
            fingerprint, actorUserId, now);
        // BE-RWI-175: catatan pemesanan dari bangsal (opsional) ikut sebagai alasan histori Request.
        if (note != null) requestHistory.Reason = note.Length > 1000 ? note[..1000] : note;
        entity.StatusHistories.Add(requestHistory);

        _dbContext.OprCases.Add(entity);
        try { await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException)
        {
            _dbContext.ChangeTracker.Clear();
            var concurrent = await FindIdempotentCaseAsync("Request", request.IdempotencyKey, cancellationToken);
            if (concurrent == null)
                throw new OperatingRoomConflictException("OPR002", "Tindakan sudah diproses pada kasus operasi lain.");
            EnsureSameFingerprint(concurrent.Source, fingerprint);
            return (await GetDetailAsync(concurrent.OprCaseId, cancellationToken))!;
        }
        await _loggerService.AuditAsync(LogCategory, "OperatingRoomCase.Create", "Membuat permintaan kasus operasi.",
            new { entity.Id, entity.CaseNumber, ActorUserId = actorUserId, Status = entity.Status.ToString() });
        return (await GetDetailAsync(entity.Id, cancellationToken))!;
    }

    public async Task<OprCaseDetailResponse> UpdateAsync(Guid id, UpdateOprCaseRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.Procedures, request.Indication, request.IdempotencyKey);
        var actorUserId = GetCurrentUserId();
        var actorDoctorId = _relaxation.IsRelaxed ? Guid.Empty : GetCurrentDoctorId();
        var fingerprint = BuildFingerprint(request);
        var prior = await FindIdempotentCaseAsync("UpdateRequest", request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            if (prior.OprCaseId != id)
                throw new OperatingRoomConflictException("OPR013", "Idempotency key sudah digunakan untuk kasus lain.");
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(id, cancellationToken))!;
        }

        var entity = await LoadCaseAsync(id, true, cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");
        EnsureNotRejected(entity.Status);
        if (entity.Status != OprCaseStatus.Requested)
            throw new OperatingRoomConflictException("InvalidStateTransition", "Permintaan hanya dapat diubah pada status Requested.");
        if (entity.Version != request.ExpectedVersion)
            throw new OperatingRoomConflictException("OPR012", "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
        if (!_relaxation.IsRelaxed &&
            actorDoctorId != entity.RequesterDoctorId && actorDoctorId != entity.PrimarySurgeonId)
            throw new OperatingRoomForbiddenException("Hanya dokter pemohon atau dokter bedah utama yang boleh mengubah permintaan.");

        await ValidateReferencesAsync(entity.PatientId, entity.EncounterId, request.RequesterDoctorId,
            request.PrimarySurgeonId, request.Procedures, entity.Id, cancellationToken);
        var now = DateTime.UtcNow;
        entity.RequesterDoctorId = request.RequesterDoctorId;
        entity.PrimarySurgeonId = request.PrimarySurgeonId;
        entity.CaseType = request.CaseType;
        entity.Priority = request.Priority;
        entity.Indication = request.Indication.Trim();
        entity.Laterality = Normalize(request.Laterality);
        entity.EstimatedMinutes = request.EstimatedMinutes;
        entity.PreferredAt = request.PreferredAt?.ToUniversalTime();
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;
        _dbContext.OprCaseProcedures.RemoveRange(entity.Procedures);
        entity.Procedures.Clear();
        AddProcedures(entity, request.Procedures, actorUserId, now);
        // Kasus sudah dilacak sebagai Modified. Anak baru didaftarkan lewat DbSet agar pasti
        // berstatus Added; menambahkannya lewat navigasi membuat EF mengira barisnya sudah ada.
        _dbContext.OprCaseProcedures.AddRange(entity.Procedures);
        _dbContext.OprStatusHistories.Add(NewHistory(entity.Id, entity.Status, entity.Status, "UpdateRequest",
            request.IdempotencyKey, fingerprint, actorUserId, now));

        try { await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException)
        {
            _dbContext.ChangeTracker.Clear();
            var concurrent = await FindIdempotentCaseAsync("UpdateRequest", request.IdempotencyKey, cancellationToken);
            if (concurrent != null && concurrent.OprCaseId == id)
            {
                EnsureSameFingerprint(concurrent.Source, fingerprint);
                return (await GetDetailAsync(id, cancellationToken))!;
            }
            throw new OperatingRoomConflictException("OPR012", "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
        }

        await _loggerService.AuditAsync(LogCategory, "OperatingRoomCase.Update", "Memperbarui permintaan kasus operasi.",
            new { entity.Id, entity.CaseNumber, ActorUserId = actorUserId, entity.Version });
        return (await GetDetailAsync(entity.Id, cancellationToken))!;
    }

    private async Task ValidateReferencesAsync(Guid patientId, Guid encounterId, Guid requesterDoctorId,
        Guid primarySurgeonId, IReadOnlyCollection<OprCaseProcedureRequest> procedures, Guid? currentCaseId,
        CancellationToken cancellationToken)
    {
        var encounterValid = await _dbContext.Set<RegPatientEncounter>().AsNoTracking()
            .AnyAsync(x => x.Id == encounterId && x.PatientId == patientId && !x.IsDelete, cancellationToken);
        if (!encounterValid) throw new ArgumentException("Encounter tidak ditemukan atau tidak sesuai dengan pasien.");

        var doctorIds = new[] { requesterDoctorId, primarySurgeonId }.Distinct().ToList();
        var validDoctors = await _dbContext.Set<MstDoctor>().AsNoTracking()
            .CountAsync(x => doctorIds.Contains(x.Id) && x.IsActive && !x.IsDelete, cancellationToken);
        if (validDoctors != doctorIds.Count) throw new ArgumentException("Dokter pemohon atau dokter bedah utama tidak aktif/tidak ditemukan.");

        var procedureIds = procedures.Select(x => x.PatientProcedureId).Distinct().ToList();
        var validProcedures = await _dbContext.Set<TrxPatientProcedure>().AsNoTracking()
            .CountAsync(x => procedureIds.Contains(x.Id) && x.EncounterId == encounterId && x.PatientId == patientId &&
                x.IsSurgeryRelated && x.IsActive && !x.IsDelete && !x.IsCancel, cancellationToken);
        if (validProcedures != procedureIds.Count)
            throw new ArgumentException("Tindakan tidak ditemukan, tidak aktif, atau bukan tindakan operasi.");

        var duplicateExists = await _dbContext.OprCaseProcedures.AsNoTracking()
            .AnyAsync(x => procedureIds.Contains(x.PatientProcedureId) && !x.IsDelete &&
                (!currentCaseId.HasValue || x.OprCaseId != currentCaseId.Value) && x.OprCase != null && !x.OprCase.IsDelete &&
                x.OprCase.Status != OprCaseStatus.Completed && x.OprCase.Status != OprCaseStatus.Cancelled &&
                // BE-RWI-174: order dari kasus Ditolak bebas dirujuk kasus baru ("pesan ulang").
                x.OprCase.Status != OprCaseStatus.Rejected, cancellationToken);
        if (duplicateExists)
            throw new OperatingRoomConflictException("OPR002", "Tindakan sudah diproses pada kasus operasi lain.");
    }

    private static void ValidateRequest(IReadOnlyCollection<OprCaseProcedureRequest> procedures, string indication, string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(indication)) throw new ArgumentException("Indikasi operasi wajib diisi.");
        if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new ArgumentException("Idempotency key wajib diisi.");
        if (procedures.Count == 0 || procedures.Count(x => x.IsPrimary) != 1)
            throw new ArgumentException("Pilih satu tindakan utama.");
        if (procedures.Any(x => x.PatientProcedureId == Guid.Empty) ||
            procedures.Select(x => x.PatientProcedureId).Distinct().Count() != procedures.Count)
            throw new ArgumentException("Daftar tindakan tidak valid atau memiliki data ganda.");
    }

    private static void AddProcedures(OprCase entity, IReadOnlyCollection<OprCaseProcedureRequest> procedures, Guid actorUserId, DateTime now)
    {
        var sequence = 1;
        foreach (var procedure in procedures.OrderByDescending(x => x.IsPrimary))
            entity.Procedures.Add(new OprCaseProcedure { OprCaseId = entity.Id,
                PatientProcedureId = procedure.PatientProcedureId,
                IsPrimary = procedure.IsPrimary, Sequence = sequence++, CreateDateTime = now, CreateBy = actorUserId });
    }

    private static OprStatusHistory NewHistory(Guid caseId, OprCaseStatus to, OprCaseStatus? from, string action,
        string idempotencyKey, string fingerprint, Guid actorUserId, DateTime now) => new()
    {
        OprCaseId = caseId, FromStatus = from, ToStatus = to, Action = action, ActorUserId = actorUserId,
        OccurredAt = now, Source = BuildSource(fingerprint), CorrelationId = idempotencyKey.Trim(),
        CreateDateTime = now, CreateBy = actorUserId
    };

    private Task<OprStatusHistory?> FindIdempotentCaseAsync(string action, string idempotencyKey, CancellationToken cancellationToken) =>
        _dbContext.OprStatusHistories.AsNoTracking().FirstOrDefaultAsync(x => x.Action == action &&
            x.CorrelationId == idempotencyKey.Trim() && !x.IsDelete, cancellationToken);

    private async Task<OprCase?> LoadCaseAsync(Guid id, bool tracking, CancellationToken cancellationToken)
    {
        var query = _dbContext.OprCases.Include(x => x.Patient).Include(x => x.RequesterDoctor)
            .Include(x => x.PrimarySurgeon).Include(x => x.Procedures).ThenInclude(x => x.PatientProcedure)
            .Include(x => x.StatusHistories).Where(x => x.Id == id && !x.IsDelete);
        if (!tracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    private Guid GetCurrentUserId() => GetRequiredClaim(ClaimTypes.NameIdentifier, "user_id", "Identitas pengguna tidak valid.");
    private Guid GetCurrentDoctorId() => GetRequiredClaim("doctor_id", "DoctorId", "Akun pengguna tidak terhubung dengan dokter.");

    private Guid GetRequiredClaim(string primary, string secondary, string message)
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(primary) ??
            _httpContextAccessor.HttpContext?.User.FindFirstValue(secondary);
        if (!Guid.TryParse(value, out var id) || id == Guid.Empty) throw new OperatingRoomForbiddenException(message);
        return id;
    }

    private static void EnsureDoctorActor(Guid actorDoctorId, Guid requesterDoctorId)
    {
        if (actorDoctorId != requesterDoctorId)
            throw new OperatingRoomForbiddenException("Dokter pemohon harus sesuai dengan pengguna yang sedang login.");
    }

    private static void EnsureSameFingerprint(string source, string fingerprint)
    {
        if (!string.Equals(source, BuildSource(fingerprint), StringComparison.Ordinal))
            throw new OperatingRoomConflictException("OPR013", "Idempotency key digunakan dengan isi permintaan yang berbeda.");
    }

    private static string BuildFingerprint(CreateOprCaseRequest r)
    {
        var baseline = string.Join('|', r.PatientId, r.EncounterId,
            r.RequesterDoctorId, r.PrimarySurgeonId, r.CaseType, r.Priority, r.Indication.Trim(), Normalize(r.Laterality),
            r.EstimatedMinutes, r.PreferredAt?.ToUniversalTime().Ticks, ProcedureFingerprint(r.Procedures));
        // BE-RWI-174: dua isian baru hanya ikut sidik jari bila tidak bawaan, supaya permintaan ulang
        // berkunci sama dari layar OK lama (sebelum isian ini ada) tetap dianggap permintaan yang sama.
        var serviceType = r.SurgicalServiceType ?? OprSurgicalServiceType.General;
        if (serviceType == OprSurgicalServiceType.General && !r.PlannedAnesthesiaType.HasValue)
            return Hash(baseline);
        return Hash(string.Join('|', baseline, serviceType, r.PlannedAnesthesiaType));
    }
    private static string BuildFingerprint(UpdateOprCaseRequest r) => Hash(string.Join('|', r.RequesterDoctorId,
        r.PrimarySurgeonId, r.CaseType, r.Priority, r.Indication.Trim(), Normalize(r.Laterality), r.EstimatedMinutes,
        r.PreferredAt?.ToUniversalTime().Ticks, ProcedureFingerprint(r.Procedures)));
    private static string ProcedureFingerprint(IEnumerable<OprCaseProcedureRequest> values) => string.Join(',',
        values.OrderBy(x => x.PatientProcedureId).Select(x => $"{x.PatientProcedureId:N}:{x.IsPrimary}"));
    private static Guid CreateDeterministicId(string idempotencyKey) =>
        new(SHA256.HashData(Encoding.UTF8.GetBytes($"OperatingRoomCase:{idempotencyKey.Trim()}"))[..16]);

    private static OprCaseDetailResponse MapDetail(OprCase entity)
    {
        var primary = entity.Procedures.FirstOrDefault(x => x.IsPrimary && !x.IsDelete);
        return new OprCaseDetailResponse
        {
            Id = entity.Id, CaseNumber = entity.CaseNumber, PatientId = entity.PatientId,
            PatientName = entity.Patient?.FullName ?? string.Empty, EncounterId = entity.EncounterId,
            RequesterDoctorId = entity.RequesterDoctorId, RequesterDoctorName = entity.RequesterDoctor?.FullName ?? string.Empty,
            PrimarySurgeonId = entity.PrimarySurgeonId, PrimarySurgeonName = entity.PrimarySurgeon?.FullName ?? string.Empty,
            CaseType = entity.CaseType, Priority = entity.Priority, Status = entity.Status, Outcome = entity.Outcome,
            Indication = entity.Indication, Laterality = entity.Laterality, EstimatedMinutes = entity.EstimatedMinutes,
            RequestedAt = entity.RequestedAt, PreferredAt = entity.PreferredAt, Version = entity.Version,
            PrimaryProcedureName = primary?.PatientProcedure?.ProcedureNameSnapshot ?? string.Empty,
            Procedures = entity.Procedures.Where(x => !x.IsDelete).OrderBy(x => x.Sequence).Select(x => new OprCaseProcedureResponse
            {
                PatientProcedureId = x.PatientProcedureId, ProcedureCode = x.PatientProcedure?.ProcedureCodeSnapshot ?? string.Empty,
                ProcedureName = x.PatientProcedure?.ProcedureNameSnapshot ?? string.Empty, IsPrimary = x.IsPrimary, Sequence = x.Sequence
            }).ToList(),
            AvailableActions = AvailableActions(entity.Status),
            SurgicalServiceType = entity.SurgicalServiceType,
            PlannedAnesthesiaType = entity.PlannedAnesthesiaType,
            RejectedAt = entity.RejectedAt,
            RejectionReason = entity.RejectionReason,
            LastStatusReason = entity.StatusHistories
                .Where(h => !h.IsDelete && StatusReasonActions.Contains(h.Action))
                .OrderByDescending(h => h.OccurredAt)
                .Select(h => h.Reason)
                .FirstOrDefault()
        };
    }

    /// <summary>Aksi histori yang alasannya menjadi <c>LastStatusReason</c> (tunda, batal, tolak).</summary>
    private static readonly string[] StatusReasonActions = ["Postpone", "Cancel", RejectAction];

    private const string RejectAction = "Reject";

    /// <summary>422 — hanya kasus Diminta yang dapat ditolak (API 11.5.1).</summary>
    public const string RejectNotRequestedCode = "OPR-CASE-REJ-001";

    /// <summary>422 — kasus Ditolak final dan tidak dapat diubah (API 11.5.1).</summary>
    public const string RejectedFinalCode = "OPR-CASE-REJ-002";
}
