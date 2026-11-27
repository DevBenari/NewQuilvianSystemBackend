using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Services;

/// <summary>
/// Daftar, detail, kategori, dan riwayat vendor Pengadaan (BE-PBJ-VD-003; PBJ-DEC-010, 015, 052, 053, 060).
/// </summary>
/// <remarks>
/// <para>
/// <b>Vendor = supplier Master Data.</b> Setiap <c>MstSupplier</c> yang belum dihapus tampil sebagai vendor, dengan atau
/// tanpa data Pengadaan. Identitasnya hanya dibaca. Profil Pengadaan (<see cref="PrcVendorProfile"/>) lahir saat kategori
/// pertama kali diatur, dan riwayatnya mencatat status blokir awal dari Master Data (PBJ-DEC-060).
/// </para>
/// <para>
/// <b>Status dihitung, tidak disimpan</b> (PBJ-VD-STATE bagian 1): Nonaktif di Master Data → Diblokir → Dibatasi (yang
/// tanggal berakhirnya kosong atau belum lewat) → Aktif. Contoh: dibatasi sampai 30-06-2027 → mulai 01-07-2027 tampil
/// Aktif tanpa job terjadwal.
/// </para>
/// <para>
/// <b>Keadaan dokumen</b> diambil dari dokumen terverifikasi terakhir per jenis: kedaluwarsa bila tanggal berlaku sebelum
/// hari ini, segera kedaluwarsa bila sisa ≤ <c>Procurement:Vendor:ExpiryWarningDays</c> hari (nilai awal 30,
/// PBJ-ASM-002). Jenis tanpa tanggal berlaku selalu berlaku.
/// </para>
/// <para>
/// <b>Rekening tersamarkan</b> (PBJ-DEC-015): hanya empat karakter terakhir yang keluar, mis. <c>****7890</c>; nomor utuh
/// tidak pernah masuk respons maupun log.
/// </para>
/// <para>
/// Daftar dihitung di memori setelah saringan dasar (cari, periode, kategori) dijalankan di basis data, karena status dan
/// keadaan dokumen bergantung pada tanggal operasional hari ini. Jumlah supplier rumah sakit kecil (ratusan), sehingga
/// cara ini aman; bila kelak ribuan, pindahkan perhitungan ke query.
/// </para>
/// </remarks>
public sealed class ProcurementVendorProfileService
{
    private const string LogCategory = "Corporate.ProcurementManagement.Vendor";
    private const string StaleMessage = "Data vendor sudah diubah orang lain. Muat ulang lalu ulangi perubahan Anda.";

    private readonly ApplicationDbContext _dbContext;
    private readonly ProcurementDocumentHistoryWriter _historyWriter;
    private readonly IConfiguration _configuration;
    private readonly LoggerService _loggerService;

    public ProcurementVendorProfileService(
        ApplicationDbContext dbContext,
        ProcurementDocumentHistoryWriter historyWriter,
        IConfiguration configuration,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _historyWriter = historyWriter;
        _configuration = configuration;
        _loggerService = loggerService;
    }

    private int ExpiryWarningDays => ProcurementVendorDocumentValidity.WarningDays(_configuration);

    public async Task<VendorFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken) =>
        new()
        {
            SortOptions =
            [
                new() { Value = "code", Label = "Kode" },
                new() { Value = "name", Label = "Nama" },
                new() { Value = "status", Label = "Status vendor" }
            ],
            VendorStatusOptions = new[]
                {
                    VendorStatusValues.Active, VendorStatusValues.Restricted, VendorStatusValues.Blocked, VendorStatusValues.Inactive,
                    VendorStatusValues.NonActive
                }
                .Select(x => new VendorValueOptionResponse { Value = x, Label = VendorStatusValues.Label(x) })
                .ToList(),
            DocumentStateOptions = new[]
                {
                    VendorDocumentStateValues.Valid, VendorDocumentStateValues.ExpiringSoon, VendorDocumentStateValues.Expired,
                    VendorDocumentStateValues.Pending, VendorDocumentStateValues.None
                }
                .Select(x => new VendorValueOptionResponse { Value = x, Label = VendorDocumentStateValues.Label(x) })
                .ToList(),
            ItemCategoryOptions = await _dbContext.MstProcurementItemCategories.AsNoTracking()
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.CategoryName)
                .Select(x => new VendorCategoryOptionResponse { Id = x.Id, Code = x.CategoryCode, Name = x.CategoryName, IsActive = x.IsActive })
                .ToListAsync(cancellationToken),
            BankOptions = (await _dbContext.MstSuppliers.AsNoTracking()
                    .Where(x => !x.IsDelete && x.BankName != null && x.BankName.Trim() != "")
                    .Select(x => x.BankName!.Trim())
                    .Distinct()
                    .ToListAsync(cancellationToken))
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .Select(x => new VendorValueOptionResponse { Value = x, Label = x })
                .ToList(),
            ExpiryWarningDays = ExpiryWarningDays
        };

    public async Task<VendorSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var vendors = await LoadAsync(_dbContext.MstSuppliers.AsNoTracking().Where(x => !x.IsDelete), cancellationToken);
        return new VendorSummaryResponse
        {
            Total = vendors.Count,
            Active = vendors.Count(x => x.Status == VendorStatusValues.Active),
            Restricted = vendors.Count(x => x.Status == VendorStatusValues.Restricted),
            Blocked = vendors.Count(x => x.Status == VendorStatusValues.Blocked),
            Inactive = vendors.Count(x => x.Status == VendorStatusValues.Inactive),
            ExpiredDocuments = vendors.Sum(x => x.Documents.ExpiredDocumentCount),
            ExpiringSoonDocuments = vendors.Sum(x => x.Documents.ExpiringSoonDocumentCount),
            ExpiryWarningDays = ExpiryWarningDays
        };
    }

    public async Task<PagedResult<VendorListItemResponse>> GetPagedAsync(VendorQuery request, CancellationToken cancellationToken)
    {
        var vendorStatus = NormalizeOption(request.VendorStatus, "Status vendor",
            VendorStatusValues.Active, VendorStatusValues.Restricted, VendorStatusValues.Blocked, VendorStatusValues.Inactive,
            VendorStatusValues.NonActive);
        var documentState = NormalizeOption(request.DocumentState, "Keadaan dokumen",
            VendorDocumentStateValues.Expired, VendorDocumentStateValues.ExpiringSoon, VendorDocumentStateValues.Pending,
            VendorDocumentStateValues.Valid, VendorDocumentStateValues.None);

        var query = _dbContext.MstSuppliers.AsNoTracking().Where(x => !x.IsDelete);
        query = query.WhereCreatedWithin(ProcurementPeriodFilter.Resolve(request.StartDate, request.EndDate, request.CustomPeriod));
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x =>
                x.SupplierCode.ToUpper().Contains(search) || x.SupplierName.ToUpper().Contains(search)
                || (x.LegalName != null && x.LegalName.ToUpper().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.BankName))
        {
            var bankName = request.BankName.Trim().ToUpper();
            query = query.Where(x => x.BankName != null && x.BankName.Trim().ToUpper() == bankName);
        }

        if (request.ItemCategoryId.HasValue)
        {
            var categoryId = request.ItemCategoryId.Value;
            query = query.Where(s => _dbContext.PrcVendorCategories.Any(c =>
                c.ItemCategoryId == categoryId && !c.IsDelete && c.VendorProfile!.SupplierId == s.Id && !c.VendorProfile.IsDelete));
        }

        IEnumerable<VendorSnapshot> vendors = await LoadAsync(query, cancellationToken);
        if (vendorStatus == VendorStatusValues.NonActive) vendors = vendors.Where(x => x.Status != VendorStatusValues.Active);
        else if (vendorStatus != null) vendors = vendors.Where(x => x.Status == vendorStatus);
        if (documentState != null) vendors = vendors.Where(x => x.Documents.DocumentState == documentState);

        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        vendors = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => descending
                ? vendors.OrderByDescending(x => x.Code, StringComparer.OrdinalIgnoreCase)
                : vendors.OrderBy(x => x.Code, StringComparer.OrdinalIgnoreCase),
            "status" => descending
                ? vendors.OrderByDescending(x => StatusRank(x.Status)).ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                : vendors.OrderBy(x => StatusRank(x.Status)).ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase),
            _ => descending
                ? vendors.OrderByDescending(x => x.Name, StringComparer.OrdinalIgnoreCase)
                : vendors.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
        };

        var filtered = vendors.ToList();
        var items = filtered
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new VendorListItemResponse
            {
                SupplierId = x.SupplierId,
                SupplierCode = x.Code,
                SupplierName = x.Name,
                VendorStatus = x.Status,
                VendorStatusLabel = VendorStatusValues.Label(x.Status),
                Categories = x.Categories,
                DocumentState = x.Documents.DocumentState,
                DocumentStateLabel = x.Documents.DocumentStateLabel,
                ExpiredDocumentCount = x.Documents.ExpiredDocumentCount,
                ExpiringSoonDocumentCount = x.Documents.ExpiringSoonDocumentCount,
                LatestFinalScore = x.LatestEvaluations.OrderByDescending(e => e.FinalizedAt).FirstOrDefault()?.FinalScore,
                LatestEvaluationFinalizedAt = x.LatestEvaluations.OrderByDescending(e => e.FinalizedAt).FirstOrDefault()?.FinalizedAt,
                HasProcurementProfile = x.Profile != null,
                BankName = x.BankName,
                MaskedBankAccount = x.MaskedBankAccount,
                BankAccountName = x.BankAccountName,
                CreateDateTime = x.CreateDateTime
            })
            .ToList();

        return new PagedResult<VendorListItemResponse>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = filtered.Count,
            TotalPage = request.PageSize <= 0 ? 0 : (int)Math.Ceiling(filtered.Count / (double)request.PageSize),
            Items = items
        };
    }

    public async Task<VendorDetailResponse> GetDetailAsync(Guid supplierId, CancellationToken cancellationToken)
    {
        var vendor = (await LoadAsync(
                _dbContext.MstSuppliers.AsNoTracking().Where(x => x.Id == supplierId && !x.IsDelete), cancellationToken))
            .FirstOrDefault()
            ?? throw new KeyNotFoundException("Vendor tidak ditemukan atau sudah dihapus di Master Data.");

        var identity = await _dbContext.MstSuppliers.AsNoTracking()
            .Where(x => x.Id == supplierId)
            .Select(x => new
            {
                x.LegalName, x.SupplierType, x.TaxNumber, x.BusinessLicenseNumber, x.ContactPersonName, x.PhoneNumber,
                x.Email, x.Address, x.CityName, x.ProvinceName, x.BankName, x.BankAccountNumber, x.BankAccountName
            })
            .FirstAsync(cancellationToken);
        var restrictedByName = vendor.Profile?.RestrictedByUserId is Guid restrictedBy
            ? await _dbContext.Users.Where(u => u.Id == restrictedBy).Select(u => u.DisplayName).FirstOrDefaultAsync(cancellationToken)
            : null;

        return new VendorDetailResponse
        {
            SupplierId = vendor.SupplierId,
            SupplierCode = vendor.Code,
            SupplierName = vendor.Name,
            LegalName = identity.LegalName,
            SupplierType = identity.SupplierType,
            TaxNumber = identity.TaxNumber,
            BusinessLicenseNumber = identity.BusinessLicenseNumber,
            ContactPersonName = identity.ContactPersonName,
            PhoneNumber = identity.PhoneNumber,
            Email = identity.Email,
            Address = identity.Address,
            CityName = identity.CityName,
            ProvinceName = identity.ProvinceName,
            VendorStatus = vendor.Status,
            VendorStatusLabel = VendorStatusValues.Label(vendor.Status),
            IsSupplierActive = vendor.IsSupplierActive,
            IsBlacklisted = vendor.IsBlacklisted,
            BlacklistReason = vendor.BlacklistReason,
            IsRestricted = vendor.IsRestrictionInEffect,
            RestrictedUntil = vendor.IsRestrictionInEffect ? vendor.Profile?.RestrictedUntil : null,
            RestrictionReason = vendor.IsRestrictionInEffect ? vendor.Profile?.RestrictionReason : null,
            RestrictedAt = vendor.IsRestrictionInEffect ? vendor.Profile?.RestrictedAt : null,
            RestrictedByName = vendor.IsRestrictionInEffect ? restrictedByName : null,
            ProfileId = vendor.Profile?.Id,
            RowVersion = vendor.Profile?.RowVersion,
            Categories = vendor.Categories,
            Documents = vendor.Documents,
            BankAccount = new VendorBankAccountResponse
            {
                BankName = identity.BankName,
                MaskedBankAccount = MaskBankAccount(identity.BankAccountNumber),
                BankAccountName = identity.BankAccountName
            },
            LatestEvaluations = vendor.LatestEvaluations
        };
    }

    /// <summary>Riwayat vendor (PBJ-ASM-004). Vendor tanpa profil Pengadaan belum punya riwayat.</summary>
    public async Task<List<DocumentHistoryResponse>> GetHistoryAsync(Guid supplierId, CancellationToken cancellationToken)
    {
        if (!await _dbContext.MstSuppliers.AnyAsync(x => x.Id == supplierId && !x.IsDelete, cancellationToken))
            throw new KeyNotFoundException("Vendor tidak ditemukan atau sudah dihapus di Master Data.");
        var profileId = await _dbContext.PrcVendorProfiles.AsNoTracking()
            .Where(x => x.SupplierId == supplierId && !x.IsDelete)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
        return profileId.HasValue
            ? await _historyWriter.ReadAsync(ProcurementDocumentTypes.Vendor, profileId.Value, cancellationToken)
            : new List<DocumentHistoryResponse>();
    }

    /// <summary>
    /// Atur kategori vendor (<c>PUT /{supplierId}/categories</c>; PBJ-DEC-052). Profil dibuat bila belum ada, beserta riwayat
    /// status blokir awal (PBJ-DEC-060). Kategori yang baru ditambahkan harus aktif (<c>PBJ-VAL-100</c>); kategori lama yang
    /// kini nonaktif boleh tetap dipertahankan. Satu baris riwayat per kategori yang ditambah atau dilepas.
    /// </summary>
    public async Task<VendorDetailResponse> UpdateCategoriesAsync(
        Guid supplierId, UpdateVendorCategoriesRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var supplier = await _dbContext.MstSuppliers.AsNoTracking()
            .Where(x => x.Id == supplierId && !x.IsDelete)
            .Select(x => new { x.Id, x.SupplierCode, x.IsActive, x.IsBlacklisted, x.BlacklistReason })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Vendor tidak ditemukan atau sudah dihapus di Master Data.");

        var requestedIds = (request.ItemCategoryIds ?? new List<Guid>()).Where(x => x != Guid.Empty).Distinct().ToList();
        var now = DateTimeOffset.UtcNow;
        var profile = await _dbContext.PrcVendorProfiles
            .FirstOrDefaultAsync(x => x.SupplierId == supplierId && !x.IsDelete, cancellationToken);

        List<PrcVendorCategory> current = new();
        if (profile == null)
        {
            profile = AddProfile(supplierId, supplier.SupplierCode, supplier.IsActive, supplier.IsBlacklisted, supplier.BlacklistReason, actorUserId, now);
        }
        else
        {
            if (request.ExpectedRowVersion != profile.RowVersion)
                throw new ProcurementVendorConflictException("PBJ-VAL-090", StaleMessage);
            current = await _dbContext.PrcVendorCategories
                .Where(x => x.VendorProfileId == profile.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);
        }

        var currentIds = current.Select(x => x.ItemCategoryId).ToHashSet();
        var addedIds = requestedIds.Where(x => !currentIds.Contains(x)).ToList();
        var removed = current.Where(x => !requestedIds.Contains(x.ItemCategoryId)).ToList();
        var removedIds = removed.Select(x => x.ItemCategoryId).ToList();

        var categoryNames = await _dbContext.MstProcurementItemCategories.AsNoTracking()
            .Where(x => addedIds.Contains(x.Id) || removedIds.Contains(x.Id))
            .Select(x => new { x.Id, x.CategoryCode, x.CategoryName, x.IsActive, x.IsDelete })
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        foreach (var id in addedIds)
        {
            if (!categoryNames.TryGetValue(id, out var category) || category.IsDelete)
                throw new ProcurementVendorValidationException("PBJ-VAL-100", "Kategori tidak ditemukan atau sudah dihapus.");
            if (!category.IsActive)
                throw new ProcurementVendorValidationException("PBJ-VAL-100", $"Kategori {category.CategoryName} tidak aktif.");
        }

        if (addedIds.Count == 0 && removed.Count == 0 && _dbContext.Entry(profile).State != EntityState.Added)
            return await GetDetailAsync(supplierId, cancellationToken);

        foreach (var id in addedIds)
        {
            _dbContext.PrcVendorCategories.Add(new PrcVendorCategory
            {
                VendorProfileId = profile.Id,
                ItemCategoryId = id,
                CreateDateTime = now.UtcDateTime,
                CreateBy = actorUserId
            });
            _historyWriter.Record(
                ProcurementDocumentTypes.Vendor, profile.Id, supplier.SupplierCode, ProcurementDocumentHistoryActions.CategoryAdd,
                null, null, $"Kategori {categoryNames[id].CategoryName} ditambahkan.",
                new { itemCategoryId = id, code = categoryNames[id].CategoryCode }, actorUserId, now);
        }

        foreach (var row in removed)
        {
            row.IsDelete = true;
            row.DeleteDateTime = now.UtcDateTime;
            row.DeleteBy = actorUserId;
            var name = categoryNames.TryGetValue(row.ItemCategoryId, out var category) ? category.CategoryName : row.ItemCategoryId.ToString();
            _historyWriter.Record(
                ProcurementDocumentTypes.Vendor, profile.Id, supplier.SupplierCode, ProcurementDocumentHistoryActions.CategoryRemove,
                null, null, $"Kategori {name} dilepas.",
                new { itemCategoryId = row.ItemCategoryId }, actorUserId, now);
        }

        if (_dbContext.Entry(profile).State != EntityState.Added)
        {
            profile.RowVersion = Guid.NewGuid();
            profile.UpdateDateTime = now.UtcDateTime;
            profile.UpdateBy = actorUserId;
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ProcurementVendorConflictException("PBJ-VAL-090", StaleMessage);
        }
        catch (DbUpdateException exception) when ((exception.InnerException as PostgresException)?.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            // Dua orang membuat profil atau menambah kategori yang sama bersamaan.
            throw new ProcurementVendorConflictException("PBJ-VAL-090", StaleMessage);
        }

        await _loggerService.AuditAsync(LogCategory, "ProcurementVendor.UpdateCategories", "Perubahan kategori vendor Pengadaan.", new
        {
            SupplierId = supplierId,
            ProfileId = profile.Id,
            Added = addedIds.Count,
            Removed = removed.Count,
            ActorUserId = actorUserId
        });
        return await GetDetailAsync(supplierId, cancellationToken);
    }

    /// <summary>
    /// Tetapkan vendor Dibatasi (<c>POST /{supplierId}/restriction</c>; PBJ-DEC-047). Syarat: alasan wajib dan tanggal
    /// berakhir kosong atau ≥ hari ini (<c>PBJ-VAL-105</c>); vendor yang Diblokir atau sudah Dibatasi ditolak 409
    /// <c>PBJ-VAL-093</c>. Profil dibuat bila belum ada. Kunci <c>Idempotency-Key</c> yang sama mengembalikan hasil pertama.
    /// Dibatasi yang tanggal berakhirnya sudah lewat dibaca Aktif, sehingga boleh dibatasi lagi.
    /// </summary>
    public async Task<VendorDetailResponse> RestrictAsync(
        Guid supplierId, RestrictVendorRequest request, string? idempotencyKey, Guid actorUserId, CancellationToken cancellationToken)
    {
        RequireActor(actorUserId);
        var supplier = await FindSupplierAsync(supplierId, cancellationToken);
        var profile = await _dbContext.PrcVendorProfiles
            .FirstOrDefaultAsync(x => x.SupplierId == supplierId && !x.IsDelete, cancellationToken);
        if (profile != null && await _historyWriter.HasIdempotencyKeyAsync(
                ProcurementDocumentTypes.Vendor, profile.Id, ProcurementDocumentHistoryActions.Restrict, idempotencyKey, cancellationToken))
            return await GetDetailAsync(supplierId, cancellationToken);

        var reason = request.Reason?.Trim() ?? string.Empty;
        if (reason.Length == 0)
            throw new ProcurementVendorValidationException("PBJ-VAL-105", "Alasan pembatasan wajib diisi.");
        var today = DateOnly.FromDateTime(AppDateTimeHelper.OperationalDate());
        if (request.RestrictedUntil.HasValue && request.RestrictedUntil.Value < today)
            throw new ProcurementVendorValidationException("PBJ-VAL-105", "Tanggal berakhir tidak boleh sebelum hari ini.");
        if (supplier.IsBlacklisted)
            throw new ProcurementVendorConflictException("PBJ-VAL-093", "Vendor yang Diblokir tidak dapat dibatasi.");

        var now = DateTimeOffset.UtcNow;
        if (profile == null)
        {
            profile = AddProfile(supplierId, supplier.SupplierCode, supplier.IsActive, supplier.IsBlacklisted, supplier.BlacklistReason, actorUserId, now);
        }
        else
        {
            if (request.ExpectedRowVersion != profile.RowVersion)
                throw new ProcurementVendorConflictException("PBJ-VAL-090", StaleMessage);
            if (IsRestrictionInEffect(profile, today))
                throw new ProcurementVendorConflictException("PBJ-VAL-093", "Vendor sudah Dibatasi. Cabut pembatasan lebih dulu bila ingin mengubahnya.");
            profile.RowVersion = Guid.NewGuid();
            profile.UpdateDateTime = now.UtcDateTime;
            profile.UpdateBy = actorUserId;
        }

        var fromStatus = supplier.IsActive ? VendorStatusValues.Active : VendorStatusValues.Inactive;
        profile.IsRestricted = true;
        profile.RestrictedUntil = request.RestrictedUntil;
        profile.RestrictionReason = reason;
        profile.RestrictedAt = now;
        profile.RestrictedByUserId = actorUserId;
        _historyWriter.Record(
            ProcurementDocumentTypes.Vendor, profile.Id, supplier.SupplierCode, ProcurementDocumentHistoryActions.Restrict,
            fromStatus, supplier.IsActive ? VendorStatusValues.Restricted : VendorStatusValues.Inactive,
            request.RestrictedUntil.HasValue
                ? $"Dibatasi sampai {request.RestrictedUntil.Value:dd-MM-yyyy}: {reason}"
                : $"Dibatasi sampai dicabut: {reason}",
            new { idempotencyKey = idempotencyKey?.Trim(), restrictedUntil = request.RestrictedUntil }, actorUserId, now);

        await SaveProfileAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "ProcurementVendor.Restrict", "Vendor Pengadaan dibatasi.", new
        {
            SupplierId = supplierId, ProfileId = profile.Id, profile.RestrictedUntil, ActorUserId = actorUserId
        });
        return await GetDetailAsync(supplierId, cancellationToken);
    }

    /// <summary>
    /// Cabut status Dibatasi (<c>POST /{supplierId}/restriction/lift</c>; PBJ-DEC-047). Alasan wajib minimal 10 karakter
    /// (<c>PBJ-VAL-104</c>); vendor yang tidak sedang Dibatasi ditolak 409 <c>PBJ-VAL-093</c>.
    /// </summary>
    public async Task<VendorDetailResponse> LiftRestrictionAsync(
        Guid supplierId, LiftRestrictionRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        RequireActor(actorUserId);
        var supplier = await FindSupplierAsync(supplierId, cancellationToken);
        var reason = request.Reason?.Trim() ?? string.Empty;
        if (reason.Length < 10)
            throw new ProcurementVendorValidationException("PBJ-VAL-104", "Alasan wajib diisi (minimal 10 karakter).");

        var today = DateOnly.FromDateTime(AppDateTimeHelper.OperationalDate());
        var profile = await _dbContext.PrcVendorProfiles
            .FirstOrDefaultAsync(x => x.SupplierId == supplierId && !x.IsDelete, cancellationToken);
        if (profile == null || !IsRestrictionInEffect(profile, today))
            throw new ProcurementVendorConflictException("PBJ-VAL-093", "Vendor tidak sedang Dibatasi.");
        if (request.ExpectedRowVersion != profile.RowVersion)
            throw new ProcurementVendorConflictException("PBJ-VAL-090", StaleMessage);

        var now = DateTimeOffset.UtcNow;
        var fromStatus = !supplier.IsActive ? VendorStatusValues.Inactive
            : supplier.IsBlacklisted ? VendorStatusValues.Blocked
            : VendorStatusValues.Restricted;
        profile.IsRestricted = false;
        profile.RestrictedUntil = null;
        profile.RestrictionReason = null;
        profile.RestrictedAt = null;
        profile.RestrictedByUserId = null;
        profile.RowVersion = Guid.NewGuid();
        profile.UpdateDateTime = now.UtcDateTime;
        profile.UpdateBy = actorUserId;
        _historyWriter.Record(
            ProcurementDocumentTypes.Vendor, profile.Id, supplier.SupplierCode, ProcurementDocumentHistoryActions.RestrictionLift,
            fromStatus, fromStatus == VendorStatusValues.Restricted ? VendorStatusValues.Active : fromStatus,
            $"Pembatasan dicabut: {reason}", null, actorUserId, now);

        await SaveProfileAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "ProcurementVendor.LiftRestriction", "Pembatasan vendor Pengadaan dicabut.", new
        {
            SupplierId = supplierId, ProfileId = profile.Id, ActorUserId = actorUserId
        });
        return await GetDetailAsync(supplierId, cancellationToken);
    }

    private async Task<SupplierRow> FindSupplierAsync(Guid supplierId, CancellationToken cancellationToken) =>
        await _dbContext.MstSuppliers.AsNoTracking()
            .Where(x => x.Id == supplierId && !x.IsDelete)
            .Select(x => new SupplierRow(x.Id, x.SupplierCode, x.IsActive, x.IsBlacklisted, x.BlacklistReason))
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new KeyNotFoundException("Vendor tidak ditemukan atau sudah dihapus di Master Data.");

    private sealed record SupplierRow(Guid Id, string SupplierCode, bool IsActive, bool IsBlacklisted, string? BlacklistReason);

    private static bool IsRestrictionInEffect(PrcVendorProfile profile, DateOnly today) =>
        profile.IsRestricted && (!profile.RestrictedUntil.HasValue || profile.RestrictedUntil.Value >= today);

    private static void RequireActor(Guid actorUserId)
    {
        if (actorUserId == Guid.Empty)
            throw new ProcurementVendorValidationException(null, "Sesi pengguna tidak dikenali. Silakan masuk ulang.");
    }

    private async Task SaveProfileAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ProcurementVendorConflictException("PBJ-VAL-090", StaleMessage);
        }
        catch (DbUpdateException exception) when ((exception.InnerException as PostgresException)?.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ProcurementVendorConflictException("PBJ-VAL-090", StaleMessage);
        }
    }

    /// <summary>
    /// Menambahkan profil Pengadaan baru ke change tracker beserta riwayat status blokir awal dari Master Data
    /// (PBJ-DEC-060). Belum disimpan; pemanggil menyimpannya bersama perubahan lain dalam satu <c>SaveChanges</c>.
    /// Dipakai saat kategori pertama diatur dan saat dokumen pertama dicatat (BE-PBJ-VD-004).
    /// </summary>
    public PrcVendorProfile AddProfile(
        Guid supplierId, string supplierCode, bool isSupplierActive, bool isBlacklisted, string? blacklistReason,
        Guid actorUserId, DateTimeOffset now)
    {
        var profile = new PrcVendorProfile
        {
            SupplierId = supplierId,
            CreateDateTime = now.UtcDateTime,
            CreateBy = actorUserId
        };
        _dbContext.PrcVendorProfiles.Add(profile);
        var initialStatus = !isSupplierActive ? VendorStatusValues.Inactive
            : isBlacklisted ? VendorStatusValues.Blocked
            : VendorStatusValues.Active;
        _historyWriter.Record(
            ProcurementDocumentTypes.Vendor, profile.Id, supplierCode, ProcurementDocumentHistoryActions.Create,
            null, initialStatus,
            isBlacklisted
                ? $"Profil Pengadaan dibuat. Status blokir awal dari Master Data: Diblokir ({blacklistReason ?? "tanpa alasan tercatat"})."
                : "Profil Pengadaan dibuat. Status blokir awal dari Master Data: Tidak diblokir.",
            new { supplierId, isBlacklisted, isSupplierActive },
            actorUserId, now);
        return profile;
    }

    // ------------------------------------------------------------------ perhitungan status

    private sealed record ProfileRow(
        Guid Id, Guid SupplierId, bool IsRestricted, DateOnly? RestrictedUntil, string? RestrictionReason,
        DateTimeOffset? RestrictedAt, Guid? RestrictedByUserId, Guid RowVersion);

    private sealed class VendorSnapshot
    {
        public Guid SupplierId { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public bool IsSupplierActive { get; init; }
        public bool IsBlacklisted { get; init; }
        public string? BlacklistReason { get; init; }
        public DateTime CreateDateTime { get; init; }
        public string? BankName { get; init; }
        /// <summary>Sudah disamarkan di <see cref="LoadAsync"/>; nomor utuh tidak disimpan di snapshot.</summary>
        public string? MaskedBankAccount { get; init; }
        public string? BankAccountName { get; init; }
        public ProfileRow? Profile { get; init; }
        public bool IsRestrictionInEffect { get; init; }
        public string Status { get; init; } = VendorStatusValues.Active;
        public List<VendorCategoryResponse> Categories { get; init; } = new();
        public VendorDocumentSummaryResponse Documents { get; init; } = new();
        public List<VendorLatestEvaluationResponse> LatestEvaluations { get; init; } = new();
    }

    private async Task<List<VendorSnapshot>> LoadAsync(
        IQueryable<MstSupplier> supplierQuery,
        CancellationToken cancellationToken)
    {
        var suppliers = await supplierQuery
            .Select(x => new
            {
                x.Id, x.SupplierCode, x.SupplierName, x.IsActive, x.IsBlacklisted, x.BlacklistReason, x.CreateDateTime,
                x.BankName, x.BankAccountNumber, x.BankAccountName
            })
            .ToListAsync(cancellationToken);
        if (suppliers.Count == 0) return new List<VendorSnapshot>();

        var supplierIds = suppliers.Select(x => x.Id).ToList();
        var profiles = await _dbContext.PrcVendorProfiles.AsNoTracking()
            .Where(x => supplierIds.Contains(x.SupplierId) && !x.IsDelete)
            .Select(x => new ProfileRow(x.Id, x.SupplierId, x.IsRestricted, x.RestrictedUntil, x.RestrictionReason,
                x.RestrictedAt, x.RestrictedByUserId, x.RowVersion))
            .ToListAsync(cancellationToken);
        var profileBySupplier = profiles.ToDictionary(x => x.SupplierId);
        var profileIds = profiles.Select(x => x.Id).ToList();

        var categories = profileIds.Count == 0
            ? new List<(Guid ProfileId, VendorCategoryResponse Category)>()
            : (await _dbContext.PrcVendorCategories.AsNoTracking()
                    .Where(x => profileIds.Contains(x.VendorProfileId) && !x.IsDelete)
                    .Select(x => new
                    {
                        x.VendorProfileId, x.ItemCategoryId, x.ItemCategory!.CategoryCode, x.ItemCategory.CategoryName, x.ItemCategory.IsActive
                    })
                    .ToListAsync(cancellationToken))
                .Select(x => (ProfileId: x.VendorProfileId, Category: new VendorCategoryResponse
                {
                    ItemCategoryId = x.ItemCategoryId, Code = x.CategoryCode, Name = x.CategoryName, IsActive = x.IsActive
                }))
                .ToList();

        var documents = profileIds.Count == 0
            ? new List<DocumentRow>()
            : await _dbContext.PrcVendorDocuments.AsNoTracking()
                .Where(x => profileIds.Contains(x.VendorProfileId) && !x.IsDelete
                    && (x.VerificationStatus == ProcurementStatuses.VendorDocument.Verified
                        || x.VerificationStatus == ProcurementStatuses.VendorDocument.Pending))
                .Select(x => new DocumentRow(x.VendorProfileId, x.DocumentTypeId, x.VerificationStatus, x.ExpiryDate,
                    x.VerifiedAt, x.CreateDateTime))
                .ToListAsync(cancellationToken);

        var evaluations = profileIds.Count == 0
            ? new List<EvaluationRow>()
            : await _dbContext.PrcVendorEvaluations.AsNoTracking()
                .Where(x => profileIds.Contains(x.VendorProfileId) && !x.IsDelete && x.Status == ProcurementStatuses.VendorEvaluation.Final)
                .Select(x => new EvaluationRow(x.VendorProfileId, x.Id, x.ItemCategoryId, x.ItemCategory!.CategoryName, x.PeriodYear,
                    x.PeriodType, x.PeriodNo, x.FinalScore, x.Recommendation, x.FinalizedAt))
                .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(AppDateTimeHelper.OperationalDate());
        var warningDays = ExpiryWarningDays;
        var warningLimit = today.AddDays(warningDays);

        return suppliers.Select(s =>
        {
            profileBySupplier.TryGetValue(s.Id, out var profile);
            var restricted = profile != null && profile.IsRestricted
                && (!profile.RestrictedUntil.HasValue || profile.RestrictedUntil.Value >= today);
            var status = !s.IsActive ? VendorStatusValues.Inactive
                : s.IsBlacklisted ? VendorStatusValues.Blocked
                : restricted ? VendorStatusValues.Restricted
                : VendorStatusValues.Active;

            return new VendorSnapshot
            {
                SupplierId = s.Id,
                Code = s.SupplierCode,
                Name = s.SupplierName,
                IsSupplierActive = s.IsActive,
                IsBlacklisted = s.IsBlacklisted,
                BlacklistReason = s.BlacklistReason,
                CreateDateTime = s.CreateDateTime,
                BankName = s.BankName,
                MaskedBankAccount = MaskBankAccount(s.BankAccountNumber),
                BankAccountName = s.BankAccountName,
                Profile = profile,
                IsRestrictionInEffect = restricted,
                Status = status,
                Categories = profile == null
                    ? new List<VendorCategoryResponse>()
                    : categories.Where(c => c.ProfileId == profile.Id).Select(c => c.Category).OrderBy(c => c.Name).ToList(),
                Documents = SummarizeDocuments(
                    profile == null ? new List<DocumentRow>() : documents.Where(d => d.ProfileId == profile.Id).ToList(),
                    today, warningLimit, warningDays),
                LatestEvaluations = profile == null
                    ? new List<VendorLatestEvaluationResponse>()
                    : evaluations.Where(e => e.ProfileId == profile.Id)
                        .GroupBy(e => e.ItemCategoryId)
                        .Select(g => g.OrderByDescending(e => e.FinalizedAt).First())
                        .Select(e => new VendorLatestEvaluationResponse
                        {
                            EvaluationId = e.EvaluationId, ItemCategoryId = e.ItemCategoryId, ItemCategoryName = e.ItemCategoryName,
                            PeriodYear = e.PeriodYear, PeriodType = e.PeriodType, PeriodNo = e.PeriodNo, FinalScore = e.FinalScore,
                            Recommendation = e.Recommendation, FinalizedAt = e.FinalizedAt
                        })
                        .OrderBy(e => e.ItemCategoryName)
                        .ToList()
            };
        }).ToList();
    }

    private sealed record DocumentRow(
        Guid ProfileId, Guid DocumentTypeId, string VerificationStatus, DateOnly? ExpiryDate, DateTimeOffset? VerifiedAt, DateTime CreateDateTime);

    private sealed record EvaluationRow(
        Guid ProfileId, Guid EvaluationId, Guid ItemCategoryId, string ItemCategoryName, int PeriodYear, string PeriodType, int PeriodNo,
        decimal? FinalScore, string? Recommendation, DateTimeOffset? FinalizedAt);

    private static VendorDocumentSummaryResponse SummarizeDocuments(
        List<DocumentRow> documents, DateOnly today, DateOnly warningLimit, int warningDays)
    {
        // Dokumen pengganti adalah baris baru; yang dipakai adalah verifikasi terakhir per jenis.
        var effective = documents
            .Where(x => x.VerificationStatus == ProcurementStatuses.VendorDocument.Verified)
            .GroupBy(x => x.DocumentTypeId)
            .Select(g => g.OrderByDescending(x => x.VerifiedAt ?? new DateTimeOffset(x.CreateDateTime, TimeSpan.Zero)).First())
            .ToList();
        var expired = effective.Count(x => x.ExpiryDate.HasValue && x.ExpiryDate.Value < today);
        var expiring = effective.Count(x => x.ExpiryDate.HasValue && x.ExpiryDate.Value >= today && x.ExpiryDate.Value <= warningLimit);
        var pending = documents.Count(x => x.VerificationStatus == ProcurementStatuses.VendorDocument.Pending);
        var state = expired > 0 ? VendorDocumentStateValues.Expired
            : expiring > 0 ? VendorDocumentStateValues.ExpiringSoon
            : pending > 0 ? VendorDocumentStateValues.Pending
            : effective.Count > 0 ? VendorDocumentStateValues.Valid
            : VendorDocumentStateValues.None;

        return new VendorDocumentSummaryResponse
        {
            DocumentState = state,
            DocumentStateLabel = VendorDocumentStateValues.Label(state),
            VerifiedDocumentTypeCount = effective.Count,
            PendingDocumentCount = pending,
            ExpiredDocumentCount = expired,
            ExpiringSoonDocumentCount = expiring,
            ExpiryWarningDays = warningDays
        };
    }

    private static int StatusRank(string status) => status switch
    {
        VendorStatusValues.Active => 0,
        VendorStatusValues.Restricted => 1,
        VendorStatusValues.Blocked => 2,
        _ => 3
    };

    private static string? NormalizeOption(string? value, string label, params string[] allowed)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim().ToUpperInvariant();
        if (!allowed.Contains(normalized))
            throw new ProcurementVendorValidationException(null, $"{label} '{value.Trim()}' tidak dikenal.");
        return normalized;
    }

    /// <summary>Contoh: <c>1234567890</c> → <c>****7890</c>; nomor kurang dari empat karakter → <c>****</c>.</summary>
    private static string? MaskBankAccount(string? number)
    {
        if (string.IsNullOrWhiteSpace(number)) return null;
        var compact = new string(number.Where(c => !char.IsWhiteSpace(c) && c != '-' && c != '.').ToArray());
        return compact.Length >= 4 ? "****" + compact[^4..] : "****";
    }
}
