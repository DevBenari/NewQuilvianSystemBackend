using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Services
{
    /// <summary>
    /// Worklist kredensial (register masalah SDM MSL-16, 19 November 2026): daftar permohonan, worklist Mitra Bestari,
    /// worklist pleno, antrean draf dan tanda tangan SPK-RKK, radar rekredensial, registri kewenangan klinis, dan
    /// verifikasi publik SPK-RKK.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Contoh alur di rumah sakit: Sekretaris Subkomite Kredensial menunjuk dr. Budi dan dr. Sari sebagai Mitra Bestari
    /// untuk permohonan dr. Rina. Keduanya melihat permohonan itu di worklist masing-masing, menilai butir kewenangan
    /// yang diajukan, lalu Ketua Komite membuka matriks pleno yang memuat penilaian kedua reviewer. Setelah pleno,
    /// permohonan masuk antrean draf SPK-RKK, lalu Direktur Utama menandatangani dari daftar "menunggu tanda tangan".
    /// </para>
    /// <para>
    /// Penugasan reviewer tersimpan sebagai JSON di <c>TrxCredentialingApplication.ComplianceResultJson</c>
    /// (<c>ReviewerProfiles[].Id</c>), sehingga worklist reviewer mencari ID profil di JSON tersebut. Setelah pleno,
    /// JSON itu berisi BAP pleno (<c>Decision</c>, <c>PlenaryBapNumber</c>) dan tetap memuat <c>ReviewerProfiles</c> (FIN-HRD-09-BE-006).
    /// </para>
    /// </remarks>
    public interface ICredentialingWorklistQueryService
    {
        Task<PagedResult<CredentialingApplicationListItemDto>> GetApplicationsAsync(CredentialingWorklistQuery query, Guid currentUserId, CancellationToken cancellationToken = default);
        Task<PagedResult<CredentialingApplicationListItemDto>> GetPeerReviewWorklistAsync(CredentialingWorklistQuery query, Guid reviewerWorkforceProfileId, Guid currentUserId, CancellationToken cancellationToken = default);
        Task<PagedResult<CredentialingApplicationListItemDto>> GetPlenaryWorklistAsync(CredentialingWorklistQuery query, Guid currentUserId, CancellationToken cancellationToken = default);
        Task<PagedResult<CredentialingApplicationListItemDto>> GetSpkRkkDraftQueueAsync(CredentialingWorklistQuery query, Guid currentUserId, CancellationToken cancellationToken = default);
        Task<CredentialingPeerReviewSheetDto> GetPeerReviewSheetAsync(Guid applicationId, Guid reviewerWorkforceProfileId, Guid currentUserId, CancellationToken cancellationToken = default);
        Task<CredentialingPlenaryMatrixDto> GetPlenaryMatrixAsync(Guid applicationId, CancellationToken cancellationToken = default);
        Task<PagedResult<SpkRkkPendingSignatureItemDto>> GetPendingSignatureAsync(CredentialingWorklistQuery query, CancellationToken cancellationToken = default);
        Task<PagedResult<RecredentialingCycleListItemDto>> GetRecredentialingCyclesAsync(RecredentialingCycleQuery query, CancellationToken cancellationToken = default);
        Task<PagedResult<ClinicalPrivilegeRegistryItemDto>> GetPrivilegeRegistryAsync(ClinicalPrivilegeRegistryQuery query, CancellationToken cancellationToken = default);
        Task<SpkRkkPublicVerificationDto?> VerifyPublicAsync(string verificationHash, CancellationToken cancellationToken = default);
        /// <summary>True bila profil termasuk panel Mitra Bestari yang ditugaskan pada permohonan berstatus InReview.</summary>
        Task<bool> IsAssignedReviewerAsync(Guid applicationId, Guid reviewerWorkforceProfileId, CancellationToken cancellationToken = default);
    }

    public class CredentialingWorklistQueryService : ICredentialingWorklistQueryService
    {
        public const string StatusInReview = "InReview";
        public const string StatusPlenaryConcluded = "PlenarySessionConcluded";
        public const string PeerAssessmentType = "PeerReviewPortofolio";
        private const int MaxRecredentialingWindowDays = 1095;

        private readonly ApplicationDbContext _context;

        public CredentialingWorklistQueryService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private sealed class ApplicationRow
        {
            public TrxCredentialingApplication App { get; set; } = null!;
            public string? Name { get; set; }
            public string? Code { get; set; }
        }

        private IQueryable<ApplicationRow> BaseApplications() =>
            from a in _context.TrxCredentialingApplications.AsNoTracking()
            where a.IsActive && !a.IsDelete
            join p in _context.MstWorkforceProfiles.AsNoTracking() on a.WorkforceProfileId equals p.Id into pj
            from p in pj.DefaultIfEmpty()
            select new ApplicationRow
            {
                App = a,
                Name = p != null ? p.DisplayName : null,
                Code = p != null ? p.ProfileCode : null
            };

        /// <summary>
        /// Potongan JSON untuk operator <c>jsonb @&gt;</c>: permohonan yang panel reviewernya memuat profil ini.
        /// Kolom <c>ComplianceResultJson</c> bertipe <c>jsonb</c>, jadi ILIKE tidak bisa dipakai (SQLSTATE 42883).
        /// </summary>
        private static string ReviewerContainmentJson(Guid workforceProfileId) =>
            JsonSerializer.Serialize(new { ReviewerProfiles = new[] { new { Id = workforceProfileId } } });

        private const string RejectedDecisionJson = "{\"Decision\":\"Rejected\"}";

        public Task<PagedResult<CredentialingApplicationListItemDto>> GetApplicationsAsync(CredentialingWorklistQuery q, Guid currentUserId, CancellationToken cancellationToken = default)
        {
            var query = BaseApplications();
            if (!string.IsNullOrWhiteSpace(q.Status))
            {
                var status = q.Status.Trim();
                query = query.Where(x => x.App.ApplicationStatus == status);
            }

            return PageApplicationsAsync(query, q, currentUserId, cancellationToken);
        }

        public Task<PagedResult<CredentialingApplicationListItemDto>> GetPeerReviewWorklistAsync(CredentialingWorklistQuery q, Guid reviewerWorkforceProfileId, Guid currentUserId, CancellationToken cancellationToken = default)
        {
            if (reviewerWorkforceProfileId == Guid.Empty)
            {
                return Task.FromResult(HrCrossEmployeeLookup.ToPaged(Array.Empty<CredentialingApplicationListItemDto>(), 0, 1, HrCrossEmployeeLookup.NormalizePage(q.PageNumber, q.PageSize).PageSize));
            }

            var containment = ReviewerContainmentJson(reviewerWorkforceProfileId);
            var query = BaseApplications().Where(x => x.App.ApplicationStatus == StatusInReview
                && x.App.ComplianceResultJson != null
                && EF.Functions.JsonContains(x.App.ComplianceResultJson, containment));
            return PageApplicationsAsync(query, q, currentUserId, cancellationToken);
        }

        public Task<PagedResult<CredentialingApplicationListItemDto>> GetPlenaryWorklistAsync(CredentialingWorklistQuery q, Guid currentUserId, CancellationToken cancellationToken = default)
        {
            var query = BaseApplications().Where(x => x.App.ApplicationStatus == StatusInReview);
            return PageApplicationsAsync(query, q, currentUserId, cancellationToken);
        }

        public Task<PagedResult<CredentialingApplicationListItemDto>> GetSpkRkkDraftQueueAsync(CredentialingWorklistQuery q, Guid currentUserId, CancellationToken cancellationToken = default)
        {
            var issuances = _context.TrxSpkRkkIssuances.AsNoTracking();
            var query = BaseApplications().Where(x => x.App.ApplicationStatus == StatusPlenaryConcluded
                && (x.App.ComplianceResultJson == null || !EF.Functions.JsonContains(x.App.ComplianceResultJson, RejectedDecisionJson))
                && !issuances.Any(i => i.CredentialingApplicationId == x.App.Id && i.IsActive && !i.IsDelete && !i.IsRevoked));
            return PageApplicationsAsync(query, q, currentUserId, cancellationToken);
        }

        private async Task<PagedResult<CredentialingApplicationListItemDto>> PageApplicationsAsync(
            IQueryable<ApplicationRow> query, CredentialingWorklistQuery q, Guid currentUserId, CancellationToken cancellationToken)
        {
            var (page, size) = HrCrossEmployeeLookup.NormalizePage(q.PageNumber, q.PageSize);
            var pattern = HrCrossEmployeeLookup.LikePattern(q.Search);
            if (pattern != null)
            {
                query = query.Where(x => EF.Functions.ILike(x.App.ApplicationNumber, pattern)
                    || (x.Name != null && EF.Functions.ILike(x.Name, pattern))
                    || (x.Code != null && EF.Functions.ILike(x.Code, pattern)));
            }

            var total = await query.CountAsync(cancellationToken);
            var rows = await query
                .OrderByDescending(x => x.App.ApplicationDate)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(x => new
                {
                    x.App.Id,
                    x.App.ApplicationNumber,
                    x.App.WorkforceProfileId,
                    x.Name,
                    x.Code,
                    x.App.ApplicationType,
                    x.App.ApplicationStatus,
                    x.App.ApplicationDate,
                    x.App.ComplianceResultJson,
                    x.App.RequirementSnapshotJson
                })
                .ToListAsync(cancellationToken);

            var ids = rows.Select(r => r.Id).ToList();
            var assessmentStats = ids.Count == 0
                ? new Dictionary<Guid, (int Count, bool Mine)>()
                : (await _context.TrxClinicalPrivilegeAssessments.AsNoTracking()
                    .Where(a => ids.Contains(a.ClinicalPrivilegeRequestId) && a.AssessmentType == PeerAssessmentType && a.IsActive && !a.IsDelete)
                    .GroupBy(a => a.ClinicalPrivilegeRequestId)
                    .Select(g => new { g.Key, Count = g.Count(), Mine = g.Any(a => a.AssessorUserId == currentUserId) })
                    .ToListAsync(cancellationToken))
                    .ToDictionary(x => x.Key, x => (x.Count, x.Mine));

            var items = rows.Select(r =>
            {
                var snapshot = ReadComplianceSnapshot(r.ComplianceResultJson);
                assessmentStats.TryGetValue(r.Id, out var stats);
                return new CredentialingApplicationListItemDto
                {
                    Id = r.Id,
                    ApplicationNumber = r.ApplicationNumber,
                    WorkforceProfileId = r.WorkforceProfileId,
                    ApplicantName = string.IsNullOrWhiteSpace(r.Name) ? "Pegawai tidak ditemukan" : r.Name,
                    ProfileCode = r.Code,
                    ApplicationType = r.ApplicationType,
                    ApplicationStatus = r.ApplicationStatus,
                    ApplicationDate = r.ApplicationDate,
                    RequestedPrivilegeCount = ReadRequestedCatalogIds(r.RequirementSnapshotJson).Count,
                    AssignedReviewerCount = snapshot.Reviewers.Count,
                    PeerAssessmentCount = stats.Count,
                    AssessedByMe = stats.Mine,
                    PlenaryDecision = snapshot.Decision,
                    PlenaryBapNumber = snapshot.BapNumber
                };
            }).ToList();

            return HrCrossEmployeeLookup.ToPaged(items, total, page, size);
        }

        public async Task<bool> IsAssignedReviewerAsync(Guid applicationId, Guid reviewerWorkforceProfileId, CancellationToken cancellationToken = default)
        {
            if (reviewerWorkforceProfileId == Guid.Empty) return false;
            var json = await _context.TrxCredentialingApplications.AsNoTracking()
                .Where(a => a.Id == applicationId && a.IsActive && !a.IsDelete && a.ApplicationStatus == StatusInReview)
                .Select(a => a.ComplianceResultJson)
                .FirstOrDefaultAsync(cancellationToken);
            return ReadComplianceSnapshot(json).Reviewers.Any(r => r.Id == reviewerWorkforceProfileId);
        }

        public async Task<CredentialingPeerReviewSheetDto> GetPeerReviewSheetAsync(Guid applicationId, Guid reviewerWorkforceProfileId, Guid currentUserId, CancellationToken cancellationToken = default)
        {
            var row = await BaseApplications()
                .Where(x => x.App.Id == applicationId && x.App.ApplicationStatus == StatusInReview)
                .FirstOrDefaultAsync(cancellationToken);

            // Bukan anggota panel dijawab sama dengan tidak ada, agar daftar permohonan orang lain tidak bisa ditebak.
            if (row == null || reviewerWorkforceProfileId == Guid.Empty
                || !ReadComplianceSnapshot(row.App.ComplianceResultJson).Reviewers.Any(r => r.Id == reviewerWorkforceProfileId))
            {
                throw new KeyNotFoundException("Permohonan tidak ada di worklist Mitra Bestari Anda.");
            }

            var alreadyAssessed = await _context.TrxClinicalPrivilegeAssessments.AsNoTracking()
                .AnyAsync(a => a.ClinicalPrivilegeRequestId == applicationId && a.AssessmentType == PeerAssessmentType
                    && a.AssessorUserId == currentUserId && a.IsActive && !a.IsDelete, cancellationToken);

            return new CredentialingPeerReviewSheetDto
            {
                ApplicationId = row.App.Id,
                ApplicationNumber = row.App.ApplicationNumber,
                ApplicantName = row.Name ?? "Pegawai tidak ditemukan",
                ProfileCode = row.Code,
                ApplicationType = row.App.ApplicationType,
                ApplicationDate = row.App.ApplicationDate,
                ApplicantNotes = row.App.Notes,
                ReviewerWorkforceProfileId = reviewerWorkforceProfileId,
                AlreadyAssessed = alreadyAssessed,
                RequestedPrivileges = await LoadCatalogItemsAsync(ReadRequestedCatalogIds(row.App.RequirementSnapshotJson), cancellationToken)
            };
        }

        public async Task<CredentialingPlenaryMatrixDto> GetPlenaryMatrixAsync(Guid applicationId, CancellationToken cancellationToken = default)
        {
            var row = await BaseApplications().Where(x => x.App.Id == applicationId).FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException("Permohonan kredensial tidak ditemukan.");

            var assessments = await _context.TrxClinicalPrivilegeAssessments.AsNoTracking()
                .Where(a => a.ClinicalPrivilegeRequestId == applicationId && a.AssessmentType == PeerAssessmentType && a.IsActive && !a.IsDelete)
                .OrderBy(a => a.AssessmentDate)
                .Select(a => new { a.Id, a.AssessmentDate, a.Recommendation, a.RequiresSupervision, a.CompetencyResultJson })
                .ToListAsync(cancellationToken);

            var parsed = assessments.Select(a => (a, Detail: ReadAssessmentDetail(a.CompetencyResultJson))).ToList();
            var labels = await HrCrossEmployeeLookup.LoadProfileLabelsAsync(
                _context, parsed.Where(p => p.Detail.ReviewerProfileId.HasValue).Select(p => p.Detail.ReviewerProfileId!.Value), cancellationToken);

            return new CredentialingPlenaryMatrixDto
            {
                ApplicationId = row.App.Id,
                ApplicationNumber = row.App.ApplicationNumber,
                ApplicantName = row.Name ?? "Pegawai tidak ditemukan",
                ProfileCode = row.Code,
                ApplicationStatus = row.App.ApplicationStatus,
                AssignedReviewerNames = ReadComplianceSnapshot(row.App.ComplianceResultJson).Reviewers.Select(r => r.Name).ToList(),
                RequestedPrivileges = await LoadCatalogItemsAsync(ReadRequestedCatalogIds(row.App.RequirementSnapshotJson), cancellationToken),
                Assessments = parsed.Select(p => new PeerAssessmentSummaryDto
                {
                    AssessmentId = p.a.Id,
                    ReviewerWorkforceProfileId = p.Detail.ReviewerProfileId,
                    ReviewerName = p.Detail.ReviewerProfileId.HasValue
                        ? HrCrossEmployeeLookup.NameOf(labels, p.Detail.ReviewerProfileId.Value)
                        : "Reviewer tidak tercatat",
                    AssessmentDate = p.a.AssessmentDate,
                    OverallRecommendation = p.Detail.OverallNotes ?? p.a.Recommendation,
                    RequiresSupervision = p.a.RequiresSupervision,
                    Items = p.Detail.Items
                }).ToList()
            };
        }

        public async Task<PagedResult<SpkRkkPendingSignatureItemDto>> GetPendingSignatureAsync(CredentialingWorklistQuery q, CancellationToken cancellationToken = default)
        {
            var (page, size) = HrCrossEmployeeLookup.NormalizePage(q.PageNumber, q.PageSize);
            var pattern = HrCrossEmployeeLookup.LikePattern(q.Search);
            var query =
                from i in _context.TrxSpkRkkIssuances.AsNoTracking()
                where i.IsActive && !i.IsDelete && !i.IsRevoked && !i.IsSignedByDirectorGeneral && i.IssuanceStatus == "Draft"
                join p in _context.MstWorkforceProfiles.AsNoTracking() on i.WorkforceProfileId equals p.Id into pj
                from p in pj.DefaultIfEmpty()
                select new { i, Name = p != null ? p.DisplayName : null, Code = p != null ? p.ProfileCode : null };

            if (pattern != null)
            {
                query = query.Where(x => EF.Functions.ILike(x.i.SpkNumber, pattern)
                    || EF.Functions.ILike(x.i.RkkNumber, pattern)
                    || (x.Name != null && EF.Functions.ILike(x.Name, pattern))
                    || (x.Code != null && EF.Functions.ILike(x.Code, pattern)));
            }

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderBy(x => x.i.IssuedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(x => new SpkRkkPendingSignatureItemDto
                {
                    Id = x.i.Id,
                    CredentialingApplicationId = x.i.CredentialingApplicationId,
                    SpkNumber = x.i.SpkNumber,
                    RkkNumber = x.i.RkkNumber,
                    WorkforceProfileId = x.i.WorkforceProfileId,
                    HolderName = x.Name ?? "Pegawai tidak ditemukan",
                    ProfileCode = x.Code,
                    ClinicalPrivilegeType = x.i.ClinicalPrivilegeType,
                    ValidFrom = x.i.ValidFrom,
                    ValidUntil = x.i.ValidUntil,
                    IssuedAt = x.i.IssuedAt,
                    IssuanceStatus = x.i.IssuanceStatus
                })
                .ToListAsync(cancellationToken);

            return HrCrossEmployeeLookup.ToPaged(items, total, page, size);
        }

        public async Task<PagedResult<RecredentialingCycleListItemDto>> GetRecredentialingCyclesAsync(RecredentialingCycleQuery q, CancellationToken cancellationToken = default)
        {
            var (page, size) = HrCrossEmployeeLookup.NormalizePage(q.PageNumber, q.PageSize);
            var pattern = HrCrossEmployeeLookup.LikePattern(q.Search);
            var today = DateTime.UtcNow.Date;
            var windowEnd = today.AddDays(Math.Clamp(q.DueWithinDays <= 0 ? 180 : q.DueWithinDays, 1, MaxRecredentialingWindowDays));

            var query =
                from i in _context.TrxSpkRkkIssuances.AsNoTracking()
                where i.IsActive && !i.IsDelete && !i.IsRevoked && i.IsSignedByDirectorGeneral && i.ValidUntil <= windowEnd
                join p in _context.MstWorkforceProfiles.AsNoTracking() on i.WorkforceProfileId equals p.Id into pj
                from p in pj.DefaultIfEmpty()
                select new { i, Name = p != null ? p.DisplayName : null, Code = p != null ? p.ProfileCode : null };

            if (!q.IncludeExpired) query = query.Where(x => x.i.ValidUntil >= today);
            if (pattern != null)
            {
                query = query.Where(x => EF.Functions.ILike(x.i.SpkNumber, pattern)
                    || (x.Name != null && EF.Functions.ILike(x.Name, pattern))
                    || (x.Code != null && EF.Functions.ILike(x.Code, pattern)));
            }

            var total = await query.CountAsync(cancellationToken);
            var rows = await query
                .OrderBy(x => x.i.ValidUntil)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(x => new
                {
                    x.i.Id,
                    x.i.SpkNumber,
                    x.i.WorkforceProfileId,
                    x.Name,
                    x.Code,
                    x.i.ClinicalPrivilegeType,
                    x.i.ValidUntil
                })
                .ToListAsync(cancellationToken);

            var profileIds = rows.Select(r => r.WorkforceProfileId).Distinct().ToList();
            var recredentialing = profileIds.Count == 0
                ? new Dictionary<Guid, (string Status, string Number)>()
                : (await _context.TrxRecredentialingApplications.AsNoTracking()
                    .Where(r => profileIds.Contains(r.WorkforceProfileId) && r.IsActive && !r.IsDelete)
                    .Select(r => new { r.WorkforceProfileId, r.RecredentialingStatus, r.RecredentialingNumber, r.CycleNumber, r.DueDate })
                    .ToListAsync(cancellationToken))
                    .GroupBy(r => r.WorkforceProfileId)
                    .ToDictionary(g => g.Key, g =>
                    {
                        var latest = g.OrderByDescending(r => r.CycleNumber).ThenByDescending(r => r.DueDate).First();
                        return (Status: latest.RecredentialingStatus, Number: latest.RecredentialingNumber);
                    });

            var finalStatuses = new[] { "FINALIZED", "ACKNOWLEDGED" };
            var oppe = profileIds.Count == 0
                ? new Dictionary<Guid, (decimal Score, string Grade, string Period)>()
                : (await _context.HrdOppeStaffEvaluations.AsNoTracking()
                    .Where(e => profileIds.Contains(e.WorkforceProfileId) && !e.IsDelete && finalStatuses.Contains(e.Status))
                    .Select(e => new
                    {
                        e.WorkforceProfileId,
                        e.FinalCompositeScore,
                        e.PerformanceGrade,
                        PeriodName = e.Period != null ? e.Period.PeriodName : null,
                        PeriodEnd = e.Period != null ? e.Period.EndDate : (DateTime?)null
                    })
                    .ToListAsync(cancellationToken))
                    .GroupBy(e => e.WorkforceProfileId)
                    .ToDictionary(g => g.Key, g =>
                    {
                        var latest = g.OrderByDescending(e => e.PeriodEnd ?? DateTime.MinValue).First();
                        return (Score: latest.FinalCompositeScore, Grade: latest.PerformanceGrade, Period: latest.PeriodName ?? "-");
                    });

            var items = rows.Select(r =>
            {
                var hasRecred = recredentialing.TryGetValue(r.WorkforceProfileId, out var recred);
                var hasOppe = oppe.TryGetValue(r.WorkforceProfileId, out var score);
                return new RecredentialingCycleListItemDto
                {
                    SpkRkkIssuanceId = r.Id,
                    SpkNumber = r.SpkNumber,
                    WorkforceProfileId = r.WorkforceProfileId,
                    HolderName = r.Name ?? "Pegawai tidak ditemukan",
                    ProfileCode = r.Code,
                    ClinicalPrivilegeType = r.ClinicalPrivilegeType,
                    ValidUntil = r.ValidUntil,
                    DaysUntilDue = (int)Math.Floor((r.ValidUntil.Date - today).TotalDays),
                    RecredentialingStatus = hasRecred ? recred.Status : null,
                    RecredentialingNumber = hasRecred ? recred.Number : null,
                    LatestOppeScore = hasOppe ? score.Score : null,
                    LatestOppeGrade = hasOppe ? score.Grade : null,
                    LatestOppePeriodName = hasOppe ? score.Period : null
                };
            }).ToList();

            return HrCrossEmployeeLookup.ToPaged(items, total, page, size);
        }

        public async Task<PagedResult<ClinicalPrivilegeRegistryItemDto>> GetPrivilegeRegistryAsync(ClinicalPrivilegeRegistryQuery q, CancellationToken cancellationToken = default)
        {
            var (page, size) = HrCrossEmployeeLookup.NormalizePage(q.PageNumber, q.PageSize);
            var pattern = HrCrossEmployeeLookup.LikePattern(q.Search);
            var query =
                from c in _context.WfpClinicalPrivileges.AsNoTracking()
                where c.IsActive && !c.IsDelete
                join p in _context.MstWorkforceProfiles.AsNoTracking() on c.WorkforceProfileId equals p.Id into pj
                from p in pj.DefaultIfEmpty()
                select new { c, Name = p != null ? p.DisplayName : null, Code = p != null ? p.ProfileCode : null };

            if (!string.IsNullOrWhiteSpace(q.Status))
            {
                if (!Enum.TryParse<ClinicalPrivilegeStatus>(q.Status.Trim(), true, out var status))
                {
                    throw new ArgumentException("Status kewenangan klinis tidak dikenal.");
                }

                query = query.Where(x => x.c.PrivilegeStatus == status);
            }

            if (pattern != null)
            {
                query = query.Where(x => EF.Functions.ILike(x.c.PrivilegeCode, pattern)
                    || EF.Functions.ILike(x.c.PrivilegeName, pattern)
                    || (x.Name != null && EF.Functions.ILike(x.Name, pattern))
                    || (x.Code != null && EF.Functions.ILike(x.Code, pattern)));
            }

            var total = await query.CountAsync(cancellationToken);
            var rows = await query
                .OrderBy(x => x.Name)
                .ThenBy(x => x.c.PrivilegeCode)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(cancellationToken);

            var items = rows.Select(x => new ClinicalPrivilegeRegistryItemDto
            {
                Id = x.c.Id,
                WorkforceProfileId = x.c.WorkforceProfileId,
                HolderName = x.Name ?? "Pegawai tidak ditemukan",
                ProfileCode = x.Code,
                PrivilegeCode = x.c.PrivilegeCode,
                PrivilegeName = x.c.PrivilegeName,
                PrivilegeType = x.c.PrivilegeType.ToString(),
                PrivilegeStatus = x.c.PrivilegeStatus.ToString(),
                SpecialtyName = x.c.SpecialtyName,
                PracticeLocation = x.c.PracticeLocation,
                EffectiveStartDate = x.c.EffectiveStartDate,
                EffectiveEndDate = x.c.EffectiveEndDate,
                IsTemporary = x.c.IsTemporary,
                IsEmergencyPrivilege = x.c.IsEmergencyPrivilege,
                IsSupervisionRequired = x.c.IsSupervisionRequired
            }).ToList();

            return HrCrossEmployeeLookup.ToPaged(items, total, page, size);
        }

        public async Task<SpkRkkPublicVerificationDto?> VerifyPublicAsync(string verificationHash, CancellationToken cancellationToken = default)
        {
            var hash = (verificationHash ?? string.Empty).Trim().ToLowerInvariant();
            // Hash dibuat dari SHA-256 heksadesimal (64 karakter); bentuk lain langsung dijawab tidak ditemukan tanpa kueri.
            if (hash.Length != 64 || !hash.All(Uri.IsHexDigit)) return null;

            var row = await (
                from i in _context.TrxSpkRkkIssuances.AsNoTracking()
                // SpkRkkIssuanceService.GenerateSha256 selalu menyimpan heksadesimal huruf kecil.
                where i.VerificationHash == hash && !i.IsDelete
                join p in _context.MstWorkforceProfiles.AsNoTracking() on i.WorkforceProfileId equals p.Id into pj
                from p in pj.DefaultIfEmpty()
                select new
                {
                    i.SpkNumber,
                    i.RkkNumber,
                    i.ClinicalPrivilegeType,
                    i.ValidFrom,
                    i.ValidUntil,
                    i.SignedAt,
                    i.DirectorGeneralName,
                    i.IsSignedByDirectorGeneral,
                    i.IsSuspended,
                    i.IsRevoked,
                    i.IsActive,
                    Name = p != null ? p.DisplayName : null
                }).FirstOrDefaultAsync(cancellationToken);

            if (row == null) return null;

            var now = DateTime.UtcNow;
            var (status, label) = row switch
            {
                { IsRevoked: true } => ("Revoked", "Dicabut. Kewenangan klinis ini tidak berlaku lagi."),
                { IsActive: false } => ("Revoked", "Tidak berlaku lagi."),
                { IsSuspended: true } => ("Suspended", "Dibekukan sementara oleh Komite."),
                { IsSignedByDirectorGeneral: false } => ("NotSigned", "Belum disahkan Direktur Utama."),
                _ when row.ValidUntil < now => ("Expired", "Masa berlaku sudah habis."),
                _ when row.ValidFrom > now => ("NotYetValid", "Belum mulai berlaku."),
                _ => ("Valid", "Sah dan berlaku.")
            };

            return new SpkRkkPublicVerificationDto
            {
                IsValid = status == "Valid",
                Status = status,
                StatusLabel = label,
                HolderName = row.Name ?? "-",
                SpkNumber = row.SpkNumber,
                RkkNumber = row.RkkNumber,
                ClinicalPrivilegeType = row.ClinicalPrivilegeType,
                ValidFrom = row.ValidFrom,
                ValidUntil = row.ValidUntil,
                SignedAt = row.SignedAt,
                DirectorGeneralName = row.DirectorGeneralName
            };
        }

        private async Task<List<ClinicalPrivilegeCatalogItemDto>> LoadCatalogItemsAsync(IReadOnlyCollection<Guid> catalogIds, CancellationToken cancellationToken)
        {
            if (catalogIds.Count == 0) return new List<ClinicalPrivilegeCatalogItemDto>();
            return await _context.MstClinicalPrivilegeCatalogs.AsNoTracking()
                .Where(c => catalogIds.Contains(c.Id))
                .OrderBy(c => c.PrivilegeCode)
                .Select(c => new ClinicalPrivilegeCatalogItemDto
                {
                    Id = c.Id,
                    PrivilegeCode = c.PrivilegeCode,
                    PrivilegeName = c.PrivilegeName,
                    PrivilegeCategory = c.PrivilegeCategory,
                    ReferenceProcedureCode = c.ReferenceProcedureCode,
                    RequiresSupervision = c.RequiresSupervision,
                    AllowsIndependentPractice = c.AllowsIndependentPractice,
                    IsHighRisk = c.IsHighRisk,
                    Description = c.Description
                })
                .ToListAsync(cancellationToken);
        }

        /// <summary>ID katalog kewenangan yang diajukan (JSON array GUID di <c>RequirementSnapshotJson</c>).</summary>
        internal static List<Guid> ReadRequestedCatalogIds(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new List<Guid>();
            try
            {
                return JsonSerializer.Deserialize<List<Guid>>(json) ?? new List<Guid>();
            }
            catch (JsonException)
            {
                return new List<Guid>();
            }
        }

        internal sealed record ReviewerRef(Guid Id, string Name);

        internal sealed record ComplianceSnapshot(IReadOnlyList<ReviewerRef> Reviewers, string? Decision, string? BapNumber);

        /// <summary>Membaca JSON penugasan reviewer atau BAP pleno; JSON rusak dianggap kosong.</summary>
        internal static ComplianceSnapshot ReadComplianceSnapshot(string? json)
        {
            var reviewers = new List<ReviewerRef>();
            if (string.IsNullOrWhiteSpace(json)) return new ComplianceSnapshot(reviewers, null, null);
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object) return new ComplianceSnapshot(reviewers, null, null);
                if (root.TryGetProperty("ReviewerProfiles", out var revs) && revs.ValueKind == JsonValueKind.Array)
                {
                    foreach (var r in revs.EnumerateArray())
                    {
                        if (r.TryGetProperty("Id", out var idProp) && idProp.TryGetGuid(out var id))
                        {
                            var name = r.TryGetProperty("DisplayName", out var n) ? n.GetString() : null;
                            reviewers.Add(new ReviewerRef(id, name ?? "-"));
                        }
                    }
                }

                var decision = root.TryGetProperty("Decision", out var d) && d.ValueKind == JsonValueKind.String ? d.GetString() : null;
                var bap = root.TryGetProperty("PlenaryBapNumber", out var b) && b.ValueKind == JsonValueKind.String ? b.GetString() : null;
                return new ComplianceSnapshot(reviewers, decision, bap);
            }
            catch (JsonException)
            {
                return new ComplianceSnapshot(reviewers, null, null);
            }
        }

        private sealed record AssessmentDetail(Guid? ReviewerProfileId, string? OverallNotes, List<PrivilegeAssessmentItemDto> Items);

        private static AssessmentDetail ReadAssessmentDetail(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new AssessmentDetail(null, null, new List<PrivilegeAssessmentItemDto>());
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                Guid? reviewer = root.TryGetProperty("ReviewerProfileId", out var r) && r.TryGetGuid(out var rid) ? rid : null;
                var notes = root.TryGetProperty("OverallNotes", out var n) && n.ValueKind == JsonValueKind.String ? n.GetString() : null;
                var items = root.TryGetProperty("Items", out var i) && i.ValueKind == JsonValueKind.Array
                    ? JsonSerializer.Deserialize<List<PrivilegeAssessmentItemDto>>(i.GetRawText()) ?? new List<PrivilegeAssessmentItemDto>()
                    : new List<PrivilegeAssessmentItemDto>();
                return new AssessmentDetail(reviewer, notes, items);
            }
            catch (JsonException)
            {
                return new AssessmentDetail(null, null, new List<PrivilegeAssessmentItemDto>());
            }
        }
    }
}
