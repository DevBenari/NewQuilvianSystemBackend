using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Services;

/// <summary>
/// Dokumen legalitas vendor: catat, verifikasi, tolak, batal, daftar, riwayat (BE-PBJ-VD-004; PBJ-DEC-045, 046;
/// PBJ-VD-STATE bagian 2).
/// </summary>
/// <remarks>
/// <para>
/// <b>Alur.</b> Petugas mencatat dokumen (<c>PENDING</c>), mengunggah berkasnya lewat <c>POST /attachments</c>
/// (<c>PBJ_VENDOR_DOCUMENT</c>), lalu petugas <b>lain</b> memverifikasi atau menolaknya. Contoh: Rina mencatat Izin PBF
/// PT Sehat Medika berlaku sampai 30-06-2031 dan mengunggah PDF-nya; Budi memeriksa dan menekan Verifikasi. Bila Rina
/// sendiri yang menekan Verifikasi → 403 <c>PBJ-VAL-103</c>.
/// </para>
/// <para>
/// <c>VERIFIED</c>, <c>REJECTED</c>, dan <c>CANCELLED</c> adalah status akhir. Dokumen pengganti dicatat sebagai baris
/// baru; dokumen terverifikasi yang lebih lama dengan jenis yang sama terbaca <c>REPLACED</c>.
/// </para>
/// <para>
/// Setiap perpindahan menulis riwayat dokumen (<c>PBJ_VENDOR_DOCUMENT</c>) dan riwayat vendor (<c>PBJ_VENDOR</c>)
/// dalam satu <c>SaveChanges</c>. Data vendor tidak dibatasi per unit (PBJ-VD-PERM bagian 1).
/// </para>
/// </remarks>
public sealed class ProcurementVendorDocumentService
{
    private const string LogCategory = "Corporate.ProcurementManagement.Vendor";
    private const string NotFoundMessage = "Dokumen vendor tidak ditemukan atau sudah dihapus.";
    private const string StaleMessage = "Dokumen sudah diubah orang lain. Muat ulang lalu ulangi tindakan Anda.";
    private const string ReasonMessage = "Alasan wajib diisi (minimal 10 karakter).";
    private const int MinimumReasonLength = 10;

    private static readonly IReadOnlyDictionary<string, string> StatusLabels = new Dictionary<string, string>
    {
        [ProcurementStatuses.VendorDocument.Pending] = "Menunggu verifikasi",
        [ProcurementStatuses.VendorDocument.Verified] = "Terverifikasi",
        [ProcurementStatuses.VendorDocument.Rejected] = "Ditolak",
        [ProcurementStatuses.VendorDocument.Cancelled] = "Dibatalkan"
    };

    private readonly ApplicationDbContext _dbContext;
    private readonly ProcurementDocumentHistoryWriter _historyWriter;
    private readonly ProcurementVendorProfileService _profileService;
    private readonly AccessPermissionService _accessPermissionService;
    private readonly IConfiguration _configuration;
    private readonly LoggerService _loggerService;

    public ProcurementVendorDocumentService(
        ApplicationDbContext dbContext,
        ProcurementDocumentHistoryWriter historyWriter,
        ProcurementVendorProfileService profileService,
        AccessPermissionService accessPermissionService,
        IConfiguration configuration,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _historyWriter = historyWriter;
        _profileService = profileService;
        _accessPermissionService = accessPermissionService;
        _configuration = configuration;
        _loggerService = loggerService;
    }

    private int WarningDays => ProcurementVendorDocumentValidity.WarningDays(_configuration);

    private static DateOnly Today => DateOnly.FromDateTime(AppDateTimeHelper.OperationalDate());

    // ------------------------------------------------------------------ baca

    public async Task<VendorDocumentFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken) =>
        new()
        {
            SortOptions =
            [
                new() { Value = "createDateTime", Label = "Tanggal dicatat" },
                new() { Value = "expiryDate", Label = "Berlaku sampai" },
                new() { Value = "supplier", Label = "Vendor" },
                new() { Value = "documentType", Label = "Jenis dokumen" },
                new() { Value = "verificationStatus", Label = "Status verifikasi" }
            ],
            DocumentTypeOptions = await _dbContext.MstProcurementVendorDocumentTypes.AsNoTracking()
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.DocumentTypeName)
                .Select(x => new VendorDocumentTypeFilterOptionResponse
                {
                    Id = x.Id, Code = x.DocumentTypeCode, Name = x.DocumentTypeName,
                    RequiresExpiryDate = x.RequiresExpiryDate, IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken),
            VerificationStatusOptions = StatusLabels
                .Select(x => new VendorValueOptionResponse { Value = x.Key, Label = x.Value })
                .ToList(),
            ValidityStateOptions = ProcurementVendorDocumentValidity.All
                .Select(x => new VendorValueOptionResponse { Value = x, Label = ProcurementVendorDocumentValidity.Label(x) })
                .ToList(),
            ExpiryWarningDays = WarningDays
        };

    public async Task<VendorDocumentSummaryCountsResponse> GetSummaryAsync(Guid? supplierId, CancellationToken cancellationToken)
    {
        var query = _dbContext.PrcVendorDocuments.AsNoTracking().Where(x => !x.IsDelete && !x.VendorProfile!.IsDelete);
        if (supplierId.HasValue) query = query.Where(x => x.VendorProfile!.SupplierId == supplierId.Value);

        var rows = await query
            .Select(x => new { x.Id, x.VendorProfileId, x.DocumentTypeId, x.VerificationStatus, x.ExpiryDate, x.VerifiedAt, x.CreateDateTime })
            .ToListAsync(cancellationToken);
        var today = Today;
        var warningDays = WarningDays;
        var effective = rows
            .Where(x => x.VerificationStatus == ProcurementStatuses.VendorDocument.Verified)
            .GroupBy(x => new { x.VendorProfileId, x.DocumentTypeId })
            .Select(g => g.OrderByDescending(x => x.VerifiedAt ?? new DateTimeOffset(x.CreateDateTime, TimeSpan.Zero)).First())
            .Select(x => ProcurementVendorDocumentValidity.Resolve(x.ExpiryDate, today, warningDays))
            .ToList();

        return new VendorDocumentSummaryCountsResponse
        {
            Pending = rows.Count(x => x.VerificationStatus == ProcurementStatuses.VendorDocument.Pending),
            Verified = rows.Count(x => x.VerificationStatus == ProcurementStatuses.VendorDocument.Verified),
            Rejected = rows.Count(x => x.VerificationStatus == ProcurementStatuses.VendorDocument.Rejected),
            Expired = effective.Count(x => x == ProcurementVendorDocumentValidity.Expired),
            ExpiringSoon = effective.Count(x => x == ProcurementVendorDocumentValidity.Expiring),
            ExpiryWarningDays = warningDays
        };
    }

    public async Task<PagedResult<VendorDocumentListItemResponse>> GetPagedAsync(VendorDocumentQuery request, CancellationToken cancellationToken)
    {
        var status = NormalizeOption(request.VerificationStatus, "Status verifikasi", StatusLabels.Keys.ToArray());
        var validity = NormalizeOption(request.ValidityState, "Keadaan berlaku", ProcurementVendorDocumentValidity.All.ToArray());

        var query = _dbContext.PrcVendorDocuments.AsNoTracking().Where(x => !x.IsDelete && !x.VendorProfile!.IsDelete);
        query = query.WhereCreatedWithin(ProcurementPeriodFilter.Resolve(request.StartDate, request.EndDate, request.CustomPeriod));
        if (request.SupplierId.HasValue) query = query.Where(x => x.VendorProfile!.SupplierId == request.SupplierId.Value);
        if (request.DocumentTypeId.HasValue) query = query.Where(x => x.DocumentTypeId == request.DocumentTypeId.Value);
        if (status != null) query = query.Where(x => x.VerificationStatus == status);
        if (validity != null) query = query.Where(x => x.VerificationStatus == ProcurementStatuses.VendorDocument.Verified);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x =>
                x.VendorProfile!.Supplier!.SupplierCode.ToUpper().Contains(search)
                || x.VendorProfile.Supplier.SupplierName.ToUpper().Contains(search)
                || (x.DocumentNumber != null && x.DocumentNumber.ToUpper().Contains(search)));
        }

        var rows = await query
            .Select(x => new VendorDocumentListItemResponse
            {
                Id = x.Id,
                SupplierId = x.VendorProfile!.SupplierId,
                SupplierCode = x.VendorProfile.Supplier!.SupplierCode,
                SupplierName = x.VendorProfile.Supplier.SupplierName,
                DocumentTypeId = x.DocumentTypeId,
                DocumentTypeCode = x.DocumentType!.DocumentTypeCode,
                DocumentTypeName = x.DocumentType.DocumentTypeName,
                DocumentNumber = x.DocumentNumber,
                IssuedDate = x.IssuedDate,
                ExpiryDate = x.ExpiryDate,
                VerificationStatus = x.VerificationStatus,
                FileCount = _dbContext.PrcAttachments.Count(a =>
                    a.DocumentType == ProcurementDocumentTypes.VendorDocument && a.DocumentId == x.Id && a.IsCurrent && !a.IsDelete),
                UploadedByUserId = x.CreateBy,
                UploadedByName = _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName).FirstOrDefault(),
                CreateDateTime = x.CreateDateTime
            })
            .ToListAsync(cancellationToken);

        var latest = await LatestVerifiedIdsAsync(rows.Select(x => x.Id).ToList(), cancellationToken);
        var today = Today;
        var warningDays = WarningDays;
        foreach (var row in rows)
        {
            row.VerificationStatusLabel = StatusLabel(row.VerificationStatus);
            row.ValidityState = ValidityOf(row.Id, row.VerificationStatus, row.ExpiryDate, latest, today, warningDays);
            row.ValidityStateLabel = row.ValidityState == null ? null : ProcurementVendorDocumentValidity.Label(row.ValidityState);
            row.DaysUntilExpiry = row.ExpiryDate.HasValue ? row.ExpiryDate.Value.DayNumber - today.DayNumber : null;
        }

        IEnumerable<VendorDocumentListItemResponse> filtered = rows;
        if (validity != null) filtered = filtered.Where(x => x.ValidityState == validity);

        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        filtered = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "expirydate" => descending
                ? filtered.OrderByDescending(x => x.ExpiryDate ?? DateOnly.MaxValue)
                : filtered.OrderBy(x => x.ExpiryDate ?? DateOnly.MaxValue),
            "supplier" => descending
                ? filtered.OrderByDescending(x => x.SupplierName, StringComparer.OrdinalIgnoreCase)
                : filtered.OrderBy(x => x.SupplierName, StringComparer.OrdinalIgnoreCase),
            "documenttype" => descending
                ? filtered.OrderByDescending(x => x.DocumentTypeName, StringComparer.OrdinalIgnoreCase)
                : filtered.OrderBy(x => x.DocumentTypeName, StringComparer.OrdinalIgnoreCase),
            "verificationstatus" => descending
                ? filtered.OrderByDescending(x => x.VerificationStatus, StringComparer.Ordinal)
                : filtered.OrderBy(x => x.VerificationStatus, StringComparer.Ordinal),
            _ => descending ? filtered.OrderByDescending(x => x.CreateDateTime) : filtered.OrderBy(x => x.CreateDateTime)
        };

        var list = filtered.ToList();
        return new PagedResult<VendorDocumentListItemResponse>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = list.Count,
            TotalPage = request.PageSize <= 0 ? 0 : (int)Math.Ceiling(list.Count / (double)request.PageSize),
            Items = list.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToList()
        };
    }

    public async Task<VendorDocumentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var detail = await _dbContext.PrcVendorDocuments.AsNoTracking()
            .Where(x => x.Id == id && !x.IsDelete && !x.VendorProfile!.IsDelete)
            .Select(x => new VendorDocumentResponse
            {
                Id = x.Id,
                SupplierId = x.VendorProfile!.SupplierId,
                SupplierCode = x.VendorProfile.Supplier!.SupplierCode,
                SupplierName = x.VendorProfile.Supplier.SupplierName,
                VendorProfileId = x.VendorProfileId,
                DocumentTypeId = x.DocumentTypeId,
                DocumentTypeCode = x.DocumentType!.DocumentTypeCode,
                DocumentTypeName = x.DocumentType.DocumentTypeName,
                RequiresExpiryDate = x.DocumentType.RequiresExpiryDate,
                DocumentNumber = x.DocumentNumber,
                IssuedDate = x.IssuedDate,
                ExpiryDate = x.ExpiryDate,
                VerificationStatus = x.VerificationStatus,
                UploadedByUserId = x.CreateBy,
                UploadedByName = _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName).FirstOrDefault(),
                VerifiedByUserId = x.VerifiedByUserId,
                VerifiedByName = _dbContext.Users.Where(u => u.Id == x.VerifiedByUserId).Select(u => u.DisplayName).FirstOrDefault(),
                VerifiedAt = x.VerifiedAt,
                RejectionReason = x.RejectionReason,
                RowVersion = x.RowVersion,
                CreateDateTime = x.CreateDateTime,
                Files = _dbContext.PrcAttachments
                    .Where(a => a.DocumentType == ProcurementDocumentTypes.VendorDocument && a.DocumentId == x.Id && a.IsCurrent && !a.IsDelete)
                    .OrderBy(a => a.OriginalFileName)
                    .Select(a => new VendorDocumentFileResponse
                    {
                        AttachmentId = a.Id,
                        OriginalFileName = a.OriginalFileName,
                        ContentType = a.ContentType,
                        FileSizeBytes = a.FileSizeBytes,
                        VersionNo = a.VersionNo,
                        UploadedAt = a.CreateDateTime,
                        UploadedByName = _dbContext.Users.Where(u => u.Id == a.UploadedByUserId).Select(u => u.DisplayName).FirstOrDefault()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException(NotFoundMessage);

        var latest = await LatestVerifiedIdsAsync(new List<Guid> { id }, cancellationToken);
        var today = Today;
        detail.VerificationStatusLabel = StatusLabel(detail.VerificationStatus);
        detail.ValidityState = ValidityOf(detail.Id, detail.VerificationStatus, detail.ExpiryDate, latest, today, WarningDays);
        detail.ValidityStateLabel = detail.ValidityState == null ? null : ProcurementVendorDocumentValidity.Label(detail.ValidityState);
        detail.DaysUntilExpiry = detail.ExpiryDate.HasValue ? detail.ExpiryDate.Value.DayNumber - today.DayNumber : null;
        return detail;
    }

    public async Task<List<DocumentHistoryResponse>> GetHistoryAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await _dbContext.PrcVendorDocuments.AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken))
            throw new KeyNotFoundException(NotFoundMessage);
        return await _historyWriter.ReadAsync(ProcurementDocumentTypes.VendorDocument, id, cancellationToken);
    }

    /// <summary>
    /// Hak lampiran <c>PBJ_VENDOR_DOCUMENT</c> (BE-PBJ-004): tanpa <c>ProcurementVendorDocument : Read</c> → tidak terlihat;
    /// pengunggah boleh menambah/menarik berkas selama dokumen <c>PENDING</c>; selain itu hanya baca.
    /// </summary>
    public async Task<ProcurementDocumentAccessLevel> GetAccessLevelAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        var userId = ProcurementDataScopeService.GetUserId(user);
        if (userId == Guid.Empty
            || !await _accessPermissionService.HasAccessAsync(user, ProcurementPermissions.VendorDocumentResource, ProcurementPermissions.ReadAction))
            return ProcurementDocumentAccessLevel.None;

        var row = await _dbContext.PrcVendorDocuments.AsNoTracking()
            .Where(x => x.Id == id && !x.IsDelete && !x.VendorProfile!.IsDelete)
            .Select(x => new { x.VerificationStatus, x.CreateBy })
            .FirstOrDefaultAsync(cancellationToken);
        if (row == null) return ProcurementDocumentAccessLevel.None;
        return row.VerificationStatus == ProcurementStatuses.VendorDocument.Pending && row.CreateBy == userId
            ? ProcurementDocumentAccessLevel.Edit
            : ProcurementDocumentAccessLevel.Read;
    }

    // ------------------------------------------------------------------ tulis

    /// <summary>Catat dokumen baru berstatus <c>PENDING</c>; profil vendor dibuat bila belum ada (PBJ-DEC-060).</summary>
    public async Task<VendorDocumentResponse> CreateAsync(CreateVendorDocumentRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        RequireActor(actorUserId);
        var supplier = await _dbContext.MstSuppliers.AsNoTracking()
            .Where(x => x.Id == request.SupplierId && !x.IsDelete)
            .Select(x => new { x.Id, x.SupplierCode, x.IsActive, x.IsBlacklisted, x.BlacklistReason })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ProcurementVendorValidationException(null, "Vendor tidak ditemukan atau sudah dihapus di Master Data.");
        var documentType = await _dbContext.MstProcurementVendorDocumentTypes.AsNoTracking()
            .Where(x => x.Id == request.DocumentTypeId && !x.IsDelete)
            .Select(x => new { x.Id, x.DocumentTypeName, x.RequiresExpiryDate, x.IsActive })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ProcurementVendorValidationException(null, "Jenis dokumen tidak ditemukan atau sudah dihapus.");
        if (!documentType.IsActive)
            throw new ProcurementVendorValidationException(null, $"Jenis dokumen {documentType.DocumentTypeName} tidak aktif.");
        if (documentType.RequiresExpiryDate && !request.ExpiryDate.HasValue)
            throw new ProcurementVendorValidationException("PBJ-VAL-101", $"Tanggal berlaku wajib diisi untuk {documentType.DocumentTypeName}.");
        if (request.ExpiryDate.HasValue && request.IssuedDate.HasValue && request.ExpiryDate.Value < request.IssuedDate.Value)
            throw new ProcurementVendorValidationException("PBJ-VAL-101", "Tanggal berlaku tidak boleh sebelum tanggal terbit.");
        var documentNumber = string.IsNullOrWhiteSpace(request.DocumentNumber) ? null : request.DocumentNumber.Trim();

        var now = DateTimeOffset.UtcNow;
        var profile = await _dbContext.PrcVendorProfiles.FirstOrDefaultAsync(x => x.SupplierId == supplier.Id && !x.IsDelete, cancellationToken)
            ?? _profileService.AddProfile(supplier.Id, supplier.SupplierCode, supplier.IsActive, supplier.IsBlacklisted, supplier.BlacklistReason, actorUserId, now);

        var entity = new PrcVendorDocument
        {
            VendorProfileId = profile.Id,
            DocumentTypeId = documentType.Id,
            DocumentNumber = documentNumber,
            IssuedDate = request.IssuedDate,
            ExpiryDate = request.ExpiryDate,
            VerificationStatus = ProcurementStatuses.VendorDocument.Pending,
            CreateDateTime = now.UtcDateTime,
            CreateBy = actorUserId
        };
        _dbContext.PrcVendorDocuments.Add(entity);
        _historyWriter.Record(ProcurementDocumentTypes.VendorDocument, entity.Id, documentNumber, ProcurementDocumentHistoryActions.Create,
            null, ProcurementStatuses.VendorDocument.Pending, null,
            new { supplierId = supplier.Id, documentTypeId = documentType.Id, entity.IssuedDate, entity.ExpiryDate }, actorUserId, now);
        _historyWriter.Record(ProcurementDocumentTypes.Vendor, profile.Id, supplier.SupplierCode, ProcurementDocumentHistoryActions.DocumentAdd,
            null, null, $"Dokumen {documentType.DocumentTypeName}{NumberSuffix(documentNumber)} dicatat, menunggu verifikasi.",
            new { vendorDocumentId = entity.Id }, actorUserId, now);

        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementVendorDocument.Create", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    /// <summary>
    /// Verifikasi dokumen (<c>PENDING → VERIFIED</c>). Syarat: versi cocok, pemeriksa bukan pengunggah
    /// (<c>PBJ-VAL-103</c>), dan berkas sudah diunggah (<c>PBJ-VAL-102</c>). Kunci <c>Idempotency-Key</c> yang sama
    /// mengembalikan hasil pertama.
    /// </summary>
    public async Task<VendorDocumentResponse> VerifyAsync(
        Guid id, VerifyVendorDocumentRequest request, string? idempotencyKey, Guid actorUserId, CancellationToken cancellationToken)
    {
        RequireActor(actorUserId);
        if (await _historyWriter.HasIdempotencyKeyAsync(
                ProcurementDocumentTypes.VendorDocument, id, ProcurementDocumentHistoryActions.Verify, idempotencyKey, cancellationToken))
            return await GetByIdAsync(id, cancellationToken);

        var (entity, typeName, supplierCode) = await LoadForDecisionAsync(id, request.ExpectedRowVersion, actorUserId, cancellationToken);
        var hasFile = await _dbContext.PrcAttachments.AnyAsync(a =>
            a.DocumentType == ProcurementDocumentTypes.VendorDocument && a.DocumentId == id && a.IsCurrent && !a.IsDelete, cancellationToken);
        if (!hasFile)
            throw new ProcurementVendorValidationException("PBJ-VAL-102", "Unggah berkas dokumen sebelum diverifikasi.");

        var now = DateTimeOffset.UtcNow;
        entity.VerificationStatus = ProcurementStatuses.VendorDocument.Verified;
        entity.VerifiedByUserId = actorUserId;
        entity.VerifiedAt = now;
        Touch(entity, actorUserId, now);
        _historyWriter.Record(ProcurementDocumentTypes.VendorDocument, id, entity.DocumentNumber, ProcurementDocumentHistoryActions.Verify,
            ProcurementStatuses.VendorDocument.Pending, ProcurementStatuses.VendorDocument.Verified, null,
            new { idempotencyKey = idempotencyKey?.Trim() }, actorUserId, now);
        _historyWriter.Record(ProcurementDocumentTypes.Vendor, entity.VendorProfileId, supplierCode, ProcurementDocumentHistoryActions.DocumentVerify,
            null, null, $"Dokumen {typeName}{NumberSuffix(entity.DocumentNumber)} diverifikasi.",
            new { vendorDocumentId = id }, actorUserId, now);

        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementVendorDocument.Verify", entity, actorUserId);
        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>Tolak dokumen (<c>PENDING → REJECTED</c>) dengan alasan (<c>PBJ-VAL-104</c>); pemeriksa bukan pengunggah.</summary>
    public async Task<VendorDocumentResponse> RejectAsync(
        Guid id, RejectVendorDocumentRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        RequireActor(actorUserId);
        var reason = RequireReason(request.Reason);
        var (entity, typeName, supplierCode) = await LoadForDecisionAsync(id, request.ExpectedRowVersion, actorUserId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        entity.VerificationStatus = ProcurementStatuses.VendorDocument.Rejected;
        entity.VerifiedByUserId = actorUserId;
        entity.VerifiedAt = now;
        entity.RejectionReason = reason;
        Touch(entity, actorUserId, now);
        _historyWriter.Record(ProcurementDocumentTypes.VendorDocument, id, entity.DocumentNumber, ProcurementDocumentHistoryActions.Reject,
            ProcurementStatuses.VendorDocument.Pending, ProcurementStatuses.VendorDocument.Rejected, reason, null, actorUserId, now);
        _historyWriter.Record(ProcurementDocumentTypes.Vendor, entity.VendorProfileId, supplierCode, ProcurementDocumentHistoryActions.DocumentReject,
            null, null, $"Dokumen {typeName}{NumberSuffix(entity.DocumentNumber)} ditolak: {reason}",
            new { vendorDocumentId = id }, actorUserId, now);

        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementVendorDocument.Reject", entity, actorUserId);
        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>Batalkan dokumen <c>PENDING</c> yang salah catat; hanya oleh pengunggahnya, dengan alasan (<c>PBJ-VAL-104</c>).</summary>
    public async Task<VendorDocumentResponse> CancelAsync(
        Guid id, CancelVendorDocumentRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        RequireActor(actorUserId);
        var reason = RequireReason(request.Reason);
        var entity = await _dbContext.PrcVendorDocuments
            .Include(x => x.DocumentType)
            .Include(x => x.VendorProfile!).ThenInclude(x => x.Supplier)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete && !x.VendorProfile!.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException(NotFoundMessage);
        if (entity.VerificationStatus != ProcurementStatuses.VendorDocument.Pending)
            throw new ProcurementVendorConflictException("PBJ-VAL-093", "Hanya dokumen yang menunggu verifikasi yang dapat dibatalkan.");
        if (request.ExpectedRowVersion.HasValue && request.ExpectedRowVersion.Value != entity.RowVersion)
            throw new ProcurementVendorConflictException("PBJ-VAL-090", StaleMessage);
        if (entity.CreateBy != actorUserId)
            throw new ProcurementVendorForbiddenException(null, "Hanya pengunggah yang dapat membatalkan dokumen ini.");

        var now = DateTimeOffset.UtcNow;
        entity.VerificationStatus = ProcurementStatuses.VendorDocument.Cancelled;
        entity.RejectionReason = reason;
        entity.IsCancel = true;
        entity.CancelDateTime = now.UtcDateTime;
        entity.CancelBy = actorUserId;
        Touch(entity, actorUserId, now);
        _historyWriter.Record(ProcurementDocumentTypes.VendorDocument, id, entity.DocumentNumber, ProcurementDocumentHistoryActions.Cancel,
            ProcurementStatuses.VendorDocument.Pending, ProcurementStatuses.VendorDocument.Cancelled, reason, null, actorUserId, now);
        _historyWriter.Record(ProcurementDocumentTypes.Vendor, entity.VendorProfileId, entity.VendorProfile?.Supplier?.SupplierCode,
            ProcurementDocumentHistoryActions.DocumentCancel, null, null,
            $"Dokumen {entity.DocumentType?.DocumentTypeName}{NumberSuffix(entity.DocumentNumber)} dibatalkan: {reason}",
            new { vendorDocumentId = id }, actorUserId, now);

        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementVendorDocument.Cancel", entity, actorUserId);
        return await GetByIdAsync(id, cancellationToken);
    }

    // ------------------------------------------------------------------ bantu

    private async Task<(PrcVendorDocument Entity, string TypeName, string? SupplierCode)> LoadForDecisionAsync(
        Guid id, Guid expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.PrcVendorDocuments
            .Include(x => x.DocumentType)
            .Include(x => x.VendorProfile!).ThenInclude(x => x.Supplier)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete && !x.VendorProfile!.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException(NotFoundMessage);
        if (entity.VerificationStatus != ProcurementStatuses.VendorDocument.Pending)
            throw new ProcurementVendorConflictException("PBJ-VAL-093", "Hanya dokumen yang menunggu verifikasi yang dapat diputus.");
        if (entity.RowVersion != expectedRowVersion)
            throw new ProcurementVendorConflictException("PBJ-VAL-090", StaleMessage);
        if (entity.CreateBy == actorUserId)
            throw new ProcurementVendorForbiddenException("PBJ-VAL-103", "Dokumen ini harus diperiksa petugas lain.");
        return (entity, entity.DocumentType?.DocumentTypeName ?? string.Empty, entity.VendorProfile?.Supplier?.SupplierCode);
    }

    /// <summary>Id dokumen terverifikasi terakhir per vendor × jenis, untuk vendor-vendor milik dokumen yang diminta.</summary>
    private async Task<HashSet<Guid>> LatestVerifiedIdsAsync(List<Guid> documentIds, CancellationToken cancellationToken)
    {
        if (documentIds.Count == 0) return new HashSet<Guid>();
        var profileIds = await _dbContext.PrcVendorDocuments.AsNoTracking()
            .Where(x => documentIds.Contains(x.Id))
            .Select(x => x.VendorProfileId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var verified = await _dbContext.PrcVendorDocuments.AsNoTracking()
            .Where(x => profileIds.Contains(x.VendorProfileId) && !x.IsDelete
                && x.VerificationStatus == ProcurementStatuses.VendorDocument.Verified)
            .Select(x => new { x.Id, x.VendorProfileId, x.DocumentTypeId, x.VerifiedAt, x.CreateDateTime })
            .ToListAsync(cancellationToken);
        return verified
            .GroupBy(x => new { x.VendorProfileId, x.DocumentTypeId })
            .Select(g => g.OrderByDescending(x => x.VerifiedAt ?? new DateTimeOffset(x.CreateDateTime, TimeSpan.Zero)).First().Id)
            .ToHashSet();
    }

    private static string? ValidityOf(
        Guid id, string status, DateOnly? expiryDate, HashSet<Guid> latestVerified, DateOnly today, int warningDays)
    {
        if (status != ProcurementStatuses.VendorDocument.Verified) return null;
        return latestVerified.Contains(id)
            ? ProcurementVendorDocumentValidity.Resolve(expiryDate, today, warningDays)
            : ProcurementVendorDocumentValidity.Replaced;
    }

    private static string StatusLabel(string status) => StatusLabels.TryGetValue(status, out var label) ? label : status;

    private static string NumberSuffix(string? documentNumber) =>
        string.IsNullOrWhiteSpace(documentNumber) ? string.Empty : $" nomor {documentNumber}";

    private static string RequireReason(string? reason)
    {
        var trimmed = reason?.Trim() ?? string.Empty;
        if (trimmed.Length < MinimumReasonLength)
            throw new ProcurementVendorValidationException("PBJ-VAL-104", ReasonMessage);
        return trimmed;
    }

    private static void RequireActor(Guid actorUserId)
    {
        if (actorUserId == Guid.Empty)
            throw new ProcurementVendorForbiddenException(null, "Sesi pengguna tidak dikenali. Silakan masuk ulang.");
    }

    private static string? NormalizeOption(string? value, string label, string[] allowed)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim().ToUpperInvariant();
        if (!allowed.Contains(normalized))
            throw new ProcurementVendorValidationException(null, $"{label} '{value.Trim()}' tidak dikenal.");
        return normalized;
    }

    private static void Touch(PrcVendorDocument entity, Guid actorUserId, DateTimeOffset now)
    {
        entity.RowVersion = Guid.NewGuid();
        entity.UpdateDateTime = now.UtcDateTime;
        entity.UpdateBy = actorUserId;
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try { await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException)
        {
            throw new ProcurementVendorConflictException("PBJ-VAL-090", StaleMessage);
        }
    }

    private Task AuditAsync(string action, PrcVendorDocument entity, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan dokumen legalitas vendor.", new
        {
            EntityId = entity.Id,
            entity.VendorProfileId,
            entity.DocumentTypeId,
            entity.VerificationStatus,
            ActorUserId = actorUserId
        });
}

/// <summary>Penyedia hak lampiran <c>PBJ_VENDOR_DOCUMENT</c> untuk <see cref="ProcurementAttachmentService"/> (BE-PBJ-004).</summary>
public sealed class VendorDocumentAttachmentAccessProvider : IProcurementDocumentAccessProvider
{
    private readonly ProcurementVendorDocumentService _service;

    public VendorDocumentAttachmentAccessProvider(ProcurementVendorDocumentService service) => _service = service;

    public string DocumentType => ProcurementDocumentTypes.VendorDocument;

    public Task<ProcurementDocumentAccessLevel> GetAccessAsync(ClaimsPrincipal user, Guid documentId, CancellationToken cancellationToken) =>
        _service.GetAccessLevelAsync(user, documentId, cancellationToken);
}
