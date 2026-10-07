using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Permintaan admisi dari kamar pulih (<c>BE-RWI-181</c>, <c>RWI-DEC-201</c>, <c>INV-RWF-32</c>,
    /// state matrix 9.4). Dipanggil Kamar Operasi dalam proses yang sama saat keputusan kamar pulih
    /// disimpan (disetujui Ikbal Yulianto, <c>RWI-DEC-208</c>), dan oleh admisi saat episode dibuka.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Tidak ada admisi otomatis.</b> Service ini tidak pernah membuat episode; ia hanya mencatat
    /// bahwa pasien perlu diadmisikan, lalu petugas admisi membuka admisi berlangkah dari
    /// permintaan itu.
    /// </para>
    /// <para>
    /// Pasien yang sudah punya episode hadir tidak mendapat permintaan: jawabannya "tidak perlu,
    /// serah terima biasa" (<c>NotNeeded</c>). Contoh: Budi dirawat di Melati lalu dioperasi; keputusan
    /// kamar pulih "Rawat inap" tidak membuat permintaan karena Budi sudah punya episode hadir.
    /// </para>
    /// </remarks>
    public sealed class InpAdmissionReferralService
    {
        /// <summary>409 — admisi biasa untuk pasien yang punya permintaan Pending.</summary>
        public const string PendingReferralExistsCode = "INP-ADM-REF-001";

        /// <summary>422 — permintaan yang dirujuk sudah selesai, dibatalkan, atau bukan milik pasien.</summary>
        public const string ReferralNotPendingCode = "INP-ADM-REF-002";

        private const string LogCategory = "HealthServices.InPatientManagement.AdmissionReferral";

        private readonly ApplicationDbContext _dbContext;
        private readonly InpSettingService _settingService;
        private readonly LoggerService _loggerService;

        public InpAdmissionReferralService(ApplicationDbContext dbContext, InpSettingService settingService,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _settingService = settingService;
            _loggerService = loggerService;
        }

        // ---------------------------------------------------------------------
        // Dipanggil Kamar Operasi sesudah keputusan kamar pulih tersimpan
        // ---------------------------------------------------------------------

        /// <summary>
        /// Keputusan kamar pulih <c>Inpatient</c>/<c>Icu</c>: membuat satu permintaan <c>Pending</c>
        /// bila pasien tidak punya episode hadir dan belum punya permintaan <c>Pending</c>.
        /// Idempoten per kasus OK.
        /// </summary>
        public async Task<InpAdmissionReferralState> CreateFromRecoveryAsync(Guid caseId, Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var source = await _dbContext.OprCases.AsNoTracking()
                .Where(x => x.Id == caseId && !x.IsDelete)
                .Select(x => new { x.Id, x.PatientId, x.EncounterId, x.PrimarySurgeonId, x.CaseNumber })
                .FirstOrDefaultAsync(cancellationToken);
            if (source == null) return InpAdmissionReferralState.NotNeeded;

            var recovery = await _dbContext.OprRecoveries.AsNoTracking()
                .Where(x => x.OprCaseId == caseId && !x.IsDelete)
                .Select(x => new { x.Decision, x.DecisionNote, x.Status })
                .FirstOrDefaultAsync(cancellationToken);
            if (recovery == null || recovery.Status == OprRecoveryStatus.Monitoring ||
                recovery.Decision is not (OprRecoveryDecision.Inpatient or OprRecoveryDecision.Icu))
                return InpAdmissionReferralState.NotNeeded;

            var careLevel = recovery.Decision == OprRecoveryDecision.Icu
                ? InpRequestedCareLevel.Icu
                : InpRequestedCareLevel.Inpatient;

            // Permintaan Pending untuk kasus ini sudah ada: perbarui tingkat perawatan bila berubah.
            var existing = await _dbContext.InpAdmissionReferrals
                .FirstOrDefaultAsync(x => x.OprCaseId == caseId && x.Status == InpAdmissionReferralStatus.Pending &&
                    !x.IsDelete, cancellationToken);
            var now = DateTime.UtcNow;
            if (existing != null)
            {
                if (existing.RequestedCareLevel != careLevel || existing.RecoveryDecisionNote != Truncate(recovery.DecisionNote, 2000))
                {
                    existing.RequestedCareLevel = careLevel;
                    existing.RecoveryDecisionNote = Truncate(recovery.DecisionNote, 2000);
                    existing.RowVersion = Guid.NewGuid();
                    existing.UpdateDateTime = now;
                    existing.UpdateBy = actorUserId;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                return InpAdmissionReferralState.Created;
            }

            // Pasien yang sudah punya episode hadir cukup serah terima biasa (INV-RWF-32).
            if (await HasPresentEpisodeAsync(source.PatientId, cancellationToken))
                return InpAdmissionReferralState.NotNeeded;

            // Satu Pending per pasien: permintaan dari kasus lain sudah cukup untuk petugas admisi.
            var patientHasPending = await _dbContext.InpAdmissionReferrals.AsNoTracking()
                .AnyAsync(x => x.PatientId == source.PatientId && x.Status == InpAdmissionReferralStatus.Pending &&
                    !x.IsDelete, cancellationToken);
            if (patientHasPending) return InpAdmissionReferralState.NotNeeded;

            var referral = new InpAdmissionReferral
            {
                Id = Guid.NewGuid(),
                PatientId = source.PatientId,
                SourceEncounterId = source.EncounterId,
                OprCaseId = source.Id,
                PrimarySurgeonId = source.PrimarySurgeonId,
                RequestedCareLevel = careLevel,
                RecoveryDecisionNote = Truncate(recovery.DecisionNote, 2000),
                Status = InpAdmissionReferralStatus.Pending,
                RequestedAt = now,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = now,
                CreateBy = actorUserId
            };
            _dbContext.InpAdmissionReferrals.Add(referral);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Dua simpanan hampir bersamaan: unique parsial menolak yang kalah; hasil akhirnya
                // tetap satu permintaan Pending.
                _dbContext.Entry(referral).State = EntityState.Detached;
                var concurrent = await _dbContext.InpAdmissionReferrals.AsNoTracking()
                    .AnyAsync(x => x.OprCaseId == caseId && x.Status == InpAdmissionReferralStatus.Pending &&
                        !x.IsDelete, cancellationToken);
                return concurrent ? InpAdmissionReferralState.Created : InpAdmissionReferralState.NotNeeded;
            }

            // Catatan keputusan klinis tidak ikut ke log.
            await _loggerService.AuditAsync(LogCategory, "InpAdmissionReferral.CreateFromRecovery",
                "Membuat permintaan admisi dari keputusan kamar pulih.",
                new { ReferralId = referral.Id, OprCaseId = caseId, source.CaseNumber, referral.PatientId,
                    RequestedCareLevel = careLevel.ToString(), ActorUserId = actorUserId });
            return InpAdmissionReferralState.Created;
        }

        /// <summary>
        /// Keputusan kamar pulih berubah dari <c>Inpatient</c>/<c>Icu</c>: permintaan <c>Pending</c>
        /// kasus ini dibatalkan beralasan (<c>VAL-RWF-89</c>).
        /// </summary>
        public async Task<InpAdmissionReferralState> CancelFromRecoveryAsync(Guid caseId, string reason, Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Alasan pembatalan permintaan admisi wajib diisi.", nameof(reason));

            var referral = await _dbContext.InpAdmissionReferrals
                .FirstOrDefaultAsync(x => x.OprCaseId == caseId && x.Status == InpAdmissionReferralStatus.Pending &&
                    !x.IsDelete, cancellationToken);
            if (referral == null) return InpAdmissionReferralState.NotNeeded;

            var now = DateTime.UtcNow;
            referral.Status = InpAdmissionReferralStatus.Cancelled;
            referral.CancelledAt = now;
            referral.CancelledByUserId = actorUserId;
            referral.CancelledReason = Truncate(reason.Trim(), 500);
            referral.RowVersion = Guid.NewGuid();
            referral.UpdateDateTime = now;
            referral.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(LogCategory, "InpAdmissionReferral.CancelFromRecovery",
                "Membatalkan permintaan admisi karena keputusan kamar pulih berubah.",
                new { ReferralId = referral.Id, OprCaseId = caseId, ActorUserId = actorUserId });
            return InpAdmissionReferralState.Cancelled;
        }

        /// <summary>Keadaan permintaan kasus ini saat dibaca ulang: <c>Created</c> bila masih ada yang Pending.</summary>
        public async Task<InpAdmissionReferralState> GetStateForCaseAsync(Guid caseId,
            CancellationToken cancellationToken = default)
        {
            var pending = await _dbContext.InpAdmissionReferrals.AsNoTracking()
                .AnyAsync(x => x.OprCaseId == caseId && x.Status == InpAdmissionReferralStatus.Pending && !x.IsDelete,
                    cancellationToken);
            return pending ? InpAdmissionReferralState.Created : InpAdmissionReferralState.NotNeeded;
        }

        // ---------------------------------------------------------------------
        // Dipanggil admisi (InpEpisodeService.OpenAdmissionAsync)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Penjaga admisi (<c>VAL-RWF-87</c>, <c>VAL-RWF-88</c>). Mengembalikan permintaan yang dirujuk
        /// dalam keadaan terlacak supaya dapat diselesaikan pada transaksi admisi yang sama.
        /// </summary>
        public async Task<AdmissionReferralCheck> CheckForAdmissionAsync(Guid patientId, Guid? admissionReferralId,
            CancellationToken cancellationToken = default)
        {
            if (admissionReferralId is { } referralId && referralId != Guid.Empty)
            {
                var referral = await _dbContext.InpAdmissionReferrals
                    .FirstOrDefaultAsync(x => x.Id == referralId && !x.IsDelete, cancellationToken);
                if (referral == null)
                    return AdmissionReferralCheck.Fail(InpEpisodeOperationStatus.NotFound, null,
                        "Permintaan admisi tidak ditemukan.");
                if (referral.Status != InpAdmissionReferralStatus.Pending)
                    return AdmissionReferralCheck.Fail(InpEpisodeOperationStatus.BusinessRuleRejected, ReferralNotPendingCode,
                        "Permintaan admisi ini sudah selesai atau dibatalkan");
                if (referral.PatientId != patientId)
                    return AdmissionReferralCheck.Fail(InpEpisodeOperationStatus.BusinessRuleRejected, ReferralNotPendingCode,
                        "Permintaan admisi ini bukan milik pasien yang dipilih");
                return AdmissionReferralCheck.Ok(referral);
            }

            var hasPending = await _dbContext.InpAdmissionReferrals.AsNoTracking()
                .AnyAsync(x => x.PatientId == patientId && x.Status == InpAdmissionReferralStatus.Pending &&
                    !x.IsDelete, cancellationToken);
            return hasPending
                ? AdmissionReferralCheck.Fail(InpEpisodeOperationStatus.Conflict, PendingReferralExistsCode,
                    "Pasien punya permintaan admisi dari kamar pulih; buka admisi dari permintaan itu")
                : AdmissionReferralCheck.Ok(null);
        }

        /// <summary>
        /// Menandai permintaan selesai oleh episode yang baru dibuka. Tidak menyimpan — pemanggil
        /// menyimpan dalam transaksi admisinya.
        /// </summary>
        public static void MarkCompleted(InpAdmissionReferral referral, Guid episodeId, Guid actorUserId, DateTime now)
        {
            referral.Status = InpAdmissionReferralStatus.Completed;
            referral.CompletedEpisodeId = episodeId;
            referral.CompletedAt = now;
            referral.RowVersion = Guid.NewGuid();
            referral.UpdateDateTime = now;
            referral.UpdateBy = actorUserId;
        }

        // ---------------------------------------------------------------------
        // Bacaan (API 11.6)
        // ---------------------------------------------------------------------

        public async Task<PagedResult<AdmissionReferralResponse>> GetPagedAsync(AdmissionReferralQuery request,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);
            var status = request.Status ?? InpAdmissionReferralStatus.Pending;

            var setting = await _settingService.GetEffectiveSettingAsync(cancellationToken);
            var now = DateTime.UtcNow;
            var overdueBefore = now.AddMinutes(-setting.PendingAdmissionReferralAlertMinutes);

            var query = _dbContext.InpAdmissionReferrals.AsNoTracking()
                .Where(x => !x.IsDelete && x.Status == status);
            if (request.OverdueOnly == true)
                query = query.Where(x => x.Status == InpAdmissionReferralStatus.Pending && x.RequestedAt <= overdueBefore);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    (x.Patient != null && (x.Patient.FullName.ToLower().Contains(search) ||
                                           x.Patient.MedicalRecordNumber.ToLower().Contains(search))) ||
                    (x.OprCase != null && x.OprCase.CaseNumber.ToLower().Contains(search)));
            }

            var totalData = await query.CountAsync(cancellationToken);
            var rows = await Project(query.OrderBy(x => x.RequestedAt).ThenBy(x => x.Id)
                    .Skip((pageNumber - 1) * pageSize).Take(pageSize))
                .ToListAsync(cancellationToken);
            var encounterTypes = await ReadEncounterTypesAsync(rows, cancellationToken);

            return new PagedResult<AdmissionReferralResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = [.. rows.Select(x => Complete(x, now, overdueBefore, encounterTypes))]
            };
        }

        public async Task<AdmissionReferralResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var setting = await _settingService.GetEffectiveSettingAsync(cancellationToken);
            var now = DateTime.UtcNow;
            var row = await Project(_dbContext.InpAdmissionReferrals.AsNoTracking()
                    .Where(x => x.Id == id && !x.IsDelete))
                .FirstOrDefaultAsync(cancellationToken);
            if (row == null) return null;
            var encounterTypes = await ReadEncounterTypesAsync([row], cancellationToken);
            return Complete(row, now, now.AddMinutes(-setting.PendingAdmissionReferralAlertMinutes), encounterTypes);
        }

        /// <summary>Jenis kunjungan asal dibaca terpisah dan diubah menjadi teks di memori.</summary>
        private async Task<Dictionary<Guid, string>> ReadEncounterTypesAsync(
            IReadOnlyCollection<AdmissionReferralResponse> rows, CancellationToken cancellationToken)
        {
            var encounterIds = rows.Select(x => x.SourceEncounterId).Distinct().ToList();
            if (encounterIds.Count == 0) return new Dictionary<Guid, string>();
            var types = await _dbContext.RegPatientEncounters.AsNoTracking()
                .Where(x => encounterIds.Contains(x.Id))
                .Select(x => new { x.Id, x.EncounterType })
                .ToListAsync(cancellationToken);
            return types.ToDictionary(x => x.Id, x => x.EncounterType.ToString());
        }

        private static IQueryable<AdmissionReferralResponse> Project(IQueryable<InpAdmissionReferral> query) =>
            query.Select(x => new AdmissionReferralResponse
            {
                Id = x.Id,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                SourceEncounterId = x.SourceEncounterId,
                OprCaseId = x.OprCaseId,
                CaseNumber = x.OprCase != null ? x.OprCase.CaseNumber : string.Empty,
                ProcedureNames = x.OprCase != null
                    ? x.OprCase.Procedures.Where(p => !p.IsDelete).OrderBy(p => p.Sequence)
                        .Select(p => p.PatientProcedure != null ? p.PatientProcedure.ProcedureNameSnapshot : string.Empty)
                        .ToList()
                    : new List<string>(),
                PrimarySurgeonId = x.PrimarySurgeonId,
                PrimarySurgeonName = x.PrimarySurgeon != null ? x.PrimarySurgeon.FullName : null,
                RequestedCareLevel = x.RequestedCareLevel,
                RecoveryDecisionNote = x.RecoveryDecisionNote,
                Status = x.Status,
                RequestedAt = x.RequestedAt,
                CancelledReason = x.CancelledReason,
                CompletedEpisodeId = x.CompletedEpisodeId,
                RowVersion = x.RowVersion
            });

        private static AdmissionReferralResponse Complete(AdmissionReferralResponse row, DateTime now, DateTime overdueBefore,
            IReadOnlyDictionary<Guid, string> encounterTypes)
        {
            row.SourceEncounterType = encounterTypes.TryGetValue(row.SourceEncounterId, out var encounterType)
                ? encounterType
                : null;
            row.WaitingMinutes = row.Status == InpAdmissionReferralStatus.Pending
                ? Math.Max(0, (int)Math.Floor((now - row.RequestedAt).TotalMinutes))
                : 0;
            row.IsOverdue = row.Status == InpAdmissionReferralStatus.Pending && row.RequestedAt <= overdueBefore;
            row.ProcedureNames = [.. row.ProcedureNames.Where(x => !string.IsNullOrWhiteSpace(x))];
            return row;
        }

        private Task<bool> HasPresentEpisodeAsync(Guid patientId, CancellationToken cancellationToken) =>
            _dbContext.InpEpisodes.AsNoTracking()
                .AnyAsync(x => x.PatientId == patientId && !x.IsDelete &&
                    (x.EpisodeStatus == InpEpisodeStatus.Admitted ||
                     (x.EpisodeStatus == InpEpisodeStatus.DischargePending && x.PhysicallyLeftAt == null)),
                    cancellationToken);

        private static string? Truncate(string? value, int max) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim().Length <= max ? value.Trim() : value.Trim()[..max];
    }

    /// <summary>Hasil penjaga permintaan admisi pada admisi.</summary>
    public sealed class AdmissionReferralCheck
    {
        private AdmissionReferralCheck(InpEpisodeOperationStatus status, string? code, string? message,
            InpAdmissionReferral? referral)
        {
            Status = status;
            Code = code;
            Message = message;
            Referral = referral;
        }

        public InpEpisodeOperationStatus Status { get; }
        public string? Code { get; }
        public string? Message { get; }

        /// <summary>Permintaan yang dirujuk, terlacak; kosong bila admisi biasa.</summary>
        public InpAdmissionReferral? Referral { get; }

        public bool IsSuccess => Status == InpEpisodeOperationStatus.Success;

        public static AdmissionReferralCheck Ok(InpAdmissionReferral? referral) =>
            new(InpEpisodeOperationStatus.Success, null, null, referral);

        public static AdmissionReferralCheck Fail(InpEpisodeOperationStatus status, string? code, string message) =>
            new(status, code, message, null);
    }
}
