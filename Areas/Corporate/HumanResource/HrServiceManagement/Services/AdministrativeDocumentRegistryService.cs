using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Security;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services
{
    /// <summary>
    /// BE-HRD-385 (20 November 2026): registri dokumen administrasi lintas jenis di server — SK Kepegawaian, Surat Tugas,
    /// Surat Keterangan, dan dokumen personal (<c>WfpDocument</c>) — dengan filter, paging, keberlakuan, kerahasiaan, dan
    /// riwayat status. Dokumen rahasia hanya tampil bagi pemegang <c>WorkforceDocument : ViewConfidential</c>; akses
    /// berkas lewat endpoint terotorisasi masing-masing sumber.
    /// </summary>
    public class AdministrativeDocumentRegistryService
    {
        private static readonly string[] RegisteredSkStatuses = { "Scheduled", "Applied", "Cancelled" };
        private static readonly string[] RegisteredLetterStatuses =
        {
            AssignmentLetterStatuses.Issued, AssignmentLetterStatuses.InExecution, AssignmentLetterStatuses.Completed, AssignmentLetterStatuses.Cancelled
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly AccessPermissionService _accessPermissionService;

        public AdministrativeDocumentRegistryService(ApplicationDbContext dbContext, AccessPermissionService accessPermissionService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _accessPermissionService = accessPermissionService ?? throw new ArgumentNullException(nameof(accessPermissionService));
        }

        public async Task<PagedResult<AdministrativeDocumentRegistryItem>> GetPagedAsync(
            AdministrativeDocumentQuery query, ClaimsPrincipal user, CancellationToken cancellationToken = default)
        {
            query ??= new AdministrativeDocumentQuery();
            var (page, size) = HrCrossEmployeeLookup.NormalizePage(query.PageNumber, query.PageSize);
            var take = page * size;
            var canViewConfidential = await _accessPermissionService.HasAccessAsync(user, "WorkforceDocument", "ViewConfidential");
            var today = DateTime.UtcNow.Date;
            var pattern = HrCrossEmployeeLookup.LikePattern(query.Search);
            var from = query.StartDate.HasValue ? DateTime.SpecifyKind(query.StartDate.Value.Date, DateTimeKind.Utc) : (DateTime?)null;
            var to = query.EndDate.HasValue ? DateTime.SpecifyKind(query.EndDate.Value.Date.AddDays(1), DateTimeKind.Utc) : (DateTime?)null;
            var type = string.IsNullOrWhiteSpace(query.DocumentType) ? null : query.DocumentType.Trim();
            var validity = string.IsNullOrWhiteSpace(query.Validity) ? null : query.Validity.Trim().ToUpperInvariant();

            List<Guid>? unitProfiles = null;
            if (query.OrganizationUnitId.HasValue)
            {
                var unitId = query.OrganizationUnitId.Value;
                unitProfiles = await _dbContext.WfpOrganizationAssignments.AsNoTracking()
                    .Where(a => a.OrganizationUnitId == unitId && a.IsActive && !a.IsDelete &&
                                a.EffectiveStartDate <= today && (a.EffectiveEndDate == null || a.EffectiveEndDate >= today))
                    .Select(a => a.WorkforceProfileId)
                    .Distinct()
                    .ToListAsync(cancellationToken);
            }

            bool Wants(string source) => string.IsNullOrWhiteSpace(query.Source) || string.Equals(query.Source.Trim(), source, StringComparison.OrdinalIgnoreCase);

            var buckets = new List<(int Total, List<AdministrativeDocumentRegistryItem> Items)>();

            // ---- SK Kepegawaian
            if (Wants(AdministrativeDocumentSources.PersonnelActionOrder) && query.IsConfidential != true)
            {
                var sk = _dbContext.HrdPersonnelActionOrders.AsNoTracking()
                    .Where(x => !x.IsDelete && RegisteredSkStatuses.Contains(x.OrderStatus));
                if (query.WorkforceProfileId.HasValue) sk = sk.Where(x => x.WorkforceProfileId == query.WorkforceProfileId.Value);
                if (unitProfiles != null) sk = sk.Where(x => unitProfiles.Contains(x.WorkforceProfileId));
                if (type != null) sk = sk.Where(x => x.ActionCategory == type);
                if (from.HasValue) sk = sk.Where(x => x.EffectiveDate >= from.Value);
                if (to.HasValue) sk = sk.Where(x => x.EffectiveDate < to.Value);
                if (validity != null)
                {
                    sk = validity switch
                    {
                        AdministrativeDocumentValidity.Valid => sk.Where(x => x.OrderStatus == "Applied"),
                        AdministrativeDocumentValidity.Scheduled => sk.Where(x => x.OrderStatus == "Scheduled"),
                        AdministrativeDocumentValidity.Revoked => sk.Where(x => x.OrderStatus == "Cancelled"),
                        _ => sk.Where(x => false)
                    };
                }

                if (pattern != null)
                {
                    sk = sk.Where(x => EF.Functions.ILike(x.ActionOrderNumber, pattern)
                        || (x.WorkforceProfile != null && EF.Functions.ILike(x.WorkforceProfile.DisplayName, pattern)));
                }

                var total = await sk.CountAsync(cancellationToken);
                var rows = await sk.OrderByDescending(x => x.EffectiveDate).Take(take)
                    .Select(x => new
                    {
                        x.Id, x.ActionOrderNumber, x.ActionCategory, x.WorkforceProfileId, x.EffectiveDate, x.OrderStatus,
                        x.SubmittedAt, x.ApprovedAt, x.ExecutedAt, x.CancellationRequestedAt, x.CancellationReason, x.CounterOrderNumber,
                        Name = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : null,
                        Code = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : null
                    })
                    .ToListAsync(cancellationToken);

                buckets.Add((total, rows.Select(x =>
                {
                    var v = x.OrderStatus switch { "Applied" => AdministrativeDocumentValidity.Valid, "Scheduled" => AdministrativeDocumentValidity.Scheduled, _ => AdministrativeDocumentValidity.Revoked };
                    return Item(AdministrativeDocumentSources.PersonnelActionOrder, "SK Kepegawaian", x.Id, x.ActionOrderNumber, x.ActionCategory,
                        $"SK {x.ActionCategory}", x.WorkforceProfileId, x.Name, x.Code, x.EffectiveDate, null, null, x.OrderStatus, v, false,
                        $"/api/v1/corporate/human-resource/personnel-action-orders/{x.Id}",
                        History(("Diajukan", x.SubmittedAt, null), ("Disetujui", x.ApprovedAt, null), ("Diterapkan", x.ExecutedAt, null),
                            ("Dibatalkan", x.OrderStatus == "Cancelled" ? x.CancellationRequestedAt : null, string.Join(" ", new[] { x.CancellationReason, x.CounterOrderNumber }.Where(s => !string.IsNullOrWhiteSpace(s))))));
                }).ToList()));
            }

            // ---- Surat Tugas
            if (Wants(AdministrativeDocumentSources.AssignmentLetter) && query.IsConfidential != true)
            {
                var st = _dbContext.Set<HrdAssignmentLetter>().AsNoTracking()
                    .Where(x => !x.IsDelete && x.LetterNumber != null && RegisteredLetterStatuses.Contains(x.LetterStatus));
                if (query.WorkforceProfileId.HasValue)
                {
                    var pid = query.WorkforceProfileId.Value;
                    st = st.Where(x => x.Participants.Any(p => !p.IsDelete && p.WorkforceProfileId == pid));
                }

                if (unitProfiles != null) st = st.Where(x => x.Participants.Any(p => !p.IsDelete && unitProfiles.Contains(p.WorkforceProfileId)));
                if (type != null) st = st.Where(x => x.DutyCategory == type);
                if (from.HasValue) st = st.Where(x => x.IssuedAt >= from.Value);
                if (to.HasValue) st = st.Where(x => x.IssuedAt < to.Value);
                if (validity != null)
                {
                    st = validity switch
                    {
                        AdministrativeDocumentValidity.Valid => st.Where(x => x.LetterStatus != AssignmentLetterStatuses.Cancelled),
                        AdministrativeDocumentValidity.Revoked => st.Where(x => x.LetterStatus == AssignmentLetterStatuses.Cancelled),
                        _ => st.Where(x => false)
                    };
                }

                if (pattern != null)
                {
                    st = st.Where(x => EF.Functions.ILike(x.LetterNumber!, pattern) || EF.Functions.ILike(x.ActivityName, pattern)
                        || x.Participants.Any(p => !p.IsDelete && EF.Functions.ILike(p.DisplayNameSnapshot, pattern)));
                }

                var total = await st.CountAsync(cancellationToken);
                var rows = await st.OrderByDescending(x => x.IssuedAt).Take(take)
                    .Select(x => new
                    {
                        x.Id, x.LetterNumber, x.DutyCategory, x.ActivityName, x.IssuedAt, x.EndAt, x.LetterStatus, x.CompletedAt, x.CancelledAt, x.CancellationReason,
                        Names = x.Participants.Where(p => !p.IsDelete).OrderBy(p => p.DisplayNameSnapshot).Select(p => p.DisplayNameSnapshot).ToList()
                    })
                    .ToListAsync(cancellationToken);

                buckets.Add((total, rows.Select(x => Item(AdministrativeDocumentSources.AssignmentLetter, "Surat Tugas", x.Id, x.LetterNumber, x.DutyCategory,
                    x.ActivityName, null, x.Names.Count == 1 ? x.Names[0] : $"{x.Names.Count} peserta: {string.Join(", ", x.Names.Take(3))}{(x.Names.Count > 3 ? ", …" : string.Empty)}",
                    null, x.IssuedAt, x.EndAt, null, x.LetterStatus,
                    x.LetterStatus == AssignmentLetterStatuses.Cancelled ? AdministrativeDocumentValidity.Revoked : AdministrativeDocumentValidity.Valid, false,
                    $"/api/v1/corporate/human-resource/assignment-letters/{x.Id}/document",
                    History(("Diterbitkan", x.IssuedAt, x.LetterNumber), ("Selesai", x.CompletedAt, null), ("Dibatalkan", x.CancelledAt, x.CancellationReason)))).ToList()));
            }

            // ---- Surat Keterangan
            if (Wants(AdministrativeDocumentSources.EmployeeLetter) && (canViewConfidential || query.IsConfidential != true))
            {
                var sk2 = _dbContext.TrxEmployeeDocumentIssuances.AsNoTracking()
                    .Where(x => !x.IsDelete && (x.IssuanceStatus == "Issued" || x.IsRevoked) && x.OfficialDocumentNumber != null && x.OfficialDocumentNumber != "");
                if (!canViewConfidential) sk2 = sk2.Where(x => !x.IsFinancialSensitive);
                if (query.IsConfidential.HasValue) sk2 = sk2.Where(x => x.IsFinancialSensitive == query.IsConfidential.Value);
                if (query.WorkforceProfileId.HasValue) sk2 = sk2.Where(x => x.WorkforceProfileId == query.WorkforceProfileId.Value);
                if (unitProfiles != null) sk2 = sk2.Where(x => x.WorkforceProfileId.HasValue && unitProfiles.Contains(x.WorkforceProfileId.Value));
                if (type != null) sk2 = sk2.Where(x => x.EmployeeDocumentRequest != null && x.EmployeeDocumentRequest.EmployeeDocumentType != null && x.EmployeeDocumentRequest.EmployeeDocumentType.DocumentTypeCode == type);
                if (from.HasValue) sk2 = sk2.Where(x => x.IssuedAt >= from.Value);
                if (to.HasValue) sk2 = sk2.Where(x => x.IssuedAt < to.Value);
                if (validity != null)
                {
                    sk2 = validity switch
                    {
                        AdministrativeDocumentValidity.Valid => sk2.Where(x => !x.IsRevoked && (x.ValidUntil == null || x.ValidUntil >= today)),
                        AdministrativeDocumentValidity.Expired => sk2.Where(x => !x.IsRevoked && x.ValidUntil != null && x.ValidUntil < today),
                        AdministrativeDocumentValidity.Revoked => sk2.Where(x => x.IsRevoked),
                        _ => sk2.Where(x => false)
                    };
                }

                if (pattern != null)
                {
                    sk2 = sk2.Where(x => EF.Functions.ILike(x.OfficialDocumentNumber, pattern)
                        || (x.WorkforceProfile != null && EF.Functions.ILike(x.WorkforceProfile.DisplayName, pattern)));
                }

                var total = await sk2.CountAsync(cancellationToken);
                var rows = await sk2.OrderByDescending(x => x.IssuedAt).Take(take)
                    .Select(x => new
                    {
                        x.Id, x.OfficialDocumentNumber, x.WorkforceProfileId, x.IssuedAt, x.ValidUntil, x.VersionNumber, x.IssuanceStatus, x.IsRevoked,
                        x.RevokedAt, x.RevocationBapNumber, x.RevocationReason, x.IsFinancialSensitive, x.DigitallySignedAt,
                        TypeCode = x.EmployeeDocumentRequest != null && x.EmployeeDocumentRequest.EmployeeDocumentType != null ? x.EmployeeDocumentRequest.EmployeeDocumentType.DocumentTypeCode : null,
                        TypeName = x.EmployeeDocumentRequest != null && x.EmployeeDocumentRequest.EmployeeDocumentType != null ? x.EmployeeDocumentRequest.EmployeeDocumentType.DocumentTypeName : null,
                        Name = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : null,
                        Code = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : null
                    })
                    .ToListAsync(cancellationToken);

                buckets.Add((total, rows.Select(x =>
                {
                    var v = x.IsRevoked ? AdministrativeDocumentValidity.Revoked
                        : x.ValidUntil.HasValue && x.ValidUntil.Value < today ? AdministrativeDocumentValidity.Expired
                        : AdministrativeDocumentValidity.Valid;
                    return Item(AdministrativeDocumentSources.EmployeeLetter, "Surat Keterangan", x.Id, x.OfficialDocumentNumber, x.TypeCode ?? "-",
                        x.TypeName ?? "Surat Keterangan", x.WorkforceProfileId, x.Name, x.Code, x.IssuedAt, x.ValidUntil, x.VersionNumber,
                        x.IsRevoked ? "Revoked" : x.IssuanceStatus, v, x.IsFinancialSensitive,
                        $"/api/v1/corporate/hrd/services/documents/{x.Id}/download",
                        History(("Diterbitkan", x.DigitallySignedAt ?? x.IssuedAt, null), ("Dicabut", x.RevokedAt,
                            string.Join(" ", new[] { x.RevocationBapNumber is null ? null : $"BAP {x.RevocationBapNumber}", x.RevocationReason }.Where(s => !string.IsNullOrWhiteSpace(s))))));
                }).ToList()));
            }

            // ---- Dokumen personal (tanpa salinan arsip Surat Keterangan agar tidak tercatat dua kali)
            if (Wants(AdministrativeDocumentSources.PersonalDocument) && (canViewConfidential || query.IsConfidential != true))
            {
                var archivedIds = _dbContext.TrxEmployeeDocumentIssuances
                    .Where(i => i.EDossierDocumentId != null)
                    .Select(i => i.EDossierDocumentId!.Value);
                var docs = _dbContext.WfpDocuments.AsNoTracking().Where(x => !x.IsDelete && !archivedIds.Contains(x.Id));
                if (!canViewConfidential) docs = docs.Where(x => !x.IsConfidential);
                if (query.IsConfidential.HasValue) docs = docs.Where(x => x.IsConfidential == query.IsConfidential.Value);
                if (query.WorkforceProfileId.HasValue) docs = docs.Where(x => x.WorkforceProfileId == query.WorkforceProfileId.Value);
                if (unitProfiles != null) docs = docs.Where(x => unitProfiles.Contains(x.WorkforceProfileId));
                if (type != null) docs = docs.Where(x => x.DocumentType == type);
                if (from.HasValue) docs = docs.Where(x => (x.IssueDate ?? x.CreateDateTime) >= from.Value);
                if (to.HasValue) docs = docs.Where(x => (x.IssueDate ?? x.CreateDateTime) < to.Value);
                if (validity != null)
                {
                    docs = validity switch
                    {
                        AdministrativeDocumentValidity.Valid => docs.Where(x => x.IsActive && (x.ExpiredDate == null || x.ExpiredDate >= today)),
                        AdministrativeDocumentValidity.Expired => docs.Where(x => x.IsActive && x.ExpiredDate != null && x.ExpiredDate < today),
                        AdministrativeDocumentValidity.Revoked => docs.Where(x => !x.IsActive),
                        _ => docs.Where(x => false)
                    };
                }

                if (pattern != null)
                {
                    docs = docs.Where(x => EF.Functions.ILike(x.DocumentName, pattern)
                        || (x.DocumentNumber != null && EF.Functions.ILike(x.DocumentNumber, pattern))
                        || (x.WorkforceProfile != null && EF.Functions.ILike(x.WorkforceProfile.DisplayName, pattern)));
                }

                var total = await docs.CountAsync(cancellationToken);
                var rows = await docs.OrderByDescending(x => x.IssueDate ?? x.CreateDateTime).Take(take)
                    .Select(x => new
                    {
                        x.Id, x.DocumentNumber, x.DocumentType, x.DocumentName, x.WorkforceProfileId, x.IssueDate, x.ExpiredDate, x.IsActive,
                        x.IsConfidential, x.CreateDateTime, x.VerifiedAt, x.UpdateDateTime,
                        Name = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : null,
                        Code = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : null
                    })
                    .ToListAsync(cancellationToken);

                buckets.Add((total, rows.Select(x =>
                {
                    var v = !x.IsActive ? AdministrativeDocumentValidity.Revoked
                        : x.ExpiredDate.HasValue && x.ExpiredDate.Value < today ? AdministrativeDocumentValidity.Expired
                        : AdministrativeDocumentValidity.Valid;
                    return Item(AdministrativeDocumentSources.PersonalDocument, "Dokumen Personal", x.Id, x.DocumentNumber, x.DocumentType,
                        x.DocumentName, x.WorkforceProfileId, x.Name, x.Code, x.IssueDate ?? x.CreateDateTime, x.ExpiredDate, null,
                        x.IsActive ? "Active" : "Inactive", v, x.IsConfidential,
                        $"/api/v1/corporate/human-resource/workforce-profiles/{x.WorkforceProfileId}/documents/{x.Id}/file",
                        History(("Diunggah", x.CreateDateTime, null), ("Diverifikasi", x.VerifiedAt, null), ("Dinonaktifkan", x.IsActive ? null : x.UpdateDateTime, null)));
                }).ToList()));
            }

            var merged = buckets.SelectMany(b => b.Items)
                .OrderByDescending(i => i.DocumentDate ?? DateTime.MinValue)
                .Skip((page - 1) * size)
                .Take(size)
                .ToList();

            return HrCrossEmployeeLookup.ToPaged(merged, buckets.Sum(b => b.Total), page, size);
        }

        private static List<AdministrativeDocumentStatusHistoryEntry> History(params (string Status, DateTime? At, string? Note)[] entries) =>
            entries.Where(e => e.At.HasValue)
                .Select(e => new AdministrativeDocumentStatusHistoryEntry { Status = e.Status, At = e.At, Note = string.IsNullOrWhiteSpace(e.Note) ? null : e.Note })
                .OrderBy(e => e.At)
                .ToList();

        private static AdministrativeDocumentRegistryItem Item(
            string source, string sourceLabel, Guid id, string? number, string documentType, string title, Guid? ownerId, string? ownerName,
            string? ownerCode, DateTime? date, DateTime? validUntil, int? version, string sourceStatus, string validity, bool confidential,
            string accessPath, List<AdministrativeDocumentStatusHistoryEntry> history) => new()
        {
            Source = source,
            SourceLabel = sourceLabel,
            Id = id,
            DocumentNumber = number,
            DocumentType = documentType,
            Title = title,
            OwnerWorkforceProfileId = ownerId,
            OwnerName = string.IsNullOrWhiteSpace(ownerName) ? "Pegawai tidak ditemukan" : ownerName,
            OwnerCode = ownerCode,
            DocumentDate = date,
            ValidUntil = validUntil,
            Version = version,
            SourceStatus = sourceStatus,
            Validity = validity,
            ValidityLabel = AdministrativeDocumentValidity.Label(validity),
            IsConfidential = confidential,
            AccessLevel = confidential ? "Rahasia" : "Umum",
            AccessPath = accessPath,
            StatusHistory = history
        };
    }
}
