using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;

public sealed partial class AssetMasterDataService
{
    private const string LogCategory = "Corporate.AssetManagement.MasterData";
    private const int MaxPageSize = 100;
    private const int MaxCodeLength = 30;
    private const int MaxNameLength = 150;
    private const int MaxDescriptionLength = 500;

    private readonly ApplicationDbContext _context;
    private readonly AssetEntityResolver _entityResolver;
    private readonly LoggerService _loggerService;

    public AssetMasterDataService(
        ApplicationDbContext context,
        AssetEntityResolver entityResolver,
        LoggerService loggerService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _entityResolver = entityResolver ?? throw new ArgumentNullException(nameof(entityResolver));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
    }

    private static (int PageNumber, int PageSize, bool Descending, string SortBy) NormalizePaging(AssetMasterPagedQuery query)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 25 : Math.Min(query.PageSize, MaxPageSize);
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var sortBy = (query.SortBy ?? string.Empty).Trim().ToLowerInvariant();
        return (pageNumber, pageSize, descending, sortBy);
    }

    private static async Task<PagedResult<TResponse>> ToPagedAsync<TEntity, TResponse>(
        IQueryable<TEntity> query,
        int pageNumber,
        int pageSize,
        Func<IQueryable<TEntity>, IQueryable<TResponse>> project,
        CancellationToken cancellationToken)
    {
        var total = await query.CountAsync(cancellationToken);
        var items = await project(query.Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToListAsync(cancellationToken);
        return new PagedResult<TResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    private static IQueryable<T> Sort<T, TKey>(IQueryable<T> query, Expression<Func<T, TKey>> key, bool descending) =>
        descending ? query.OrderByDescending(key) : query.OrderBy(key);

    private static string? NormalizeSearch(string? search) =>
        string.IsNullOrWhiteSpace(search) ? null : search.Trim().ToUpper();

    private static string RequiredCode(string? value, string label)
    {
        var code = value?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(code) || code.Length > MaxCodeLength || code.Any(char.IsWhiteSpace))
        {
            throw AssetManagementException.Invalid(
                AssetMasterErrorCodes.Field,
                $"{label} wajib diisi, paling panjang {MaxCodeLength} karakter, tanpa spasi.");
        }

        return code;
    }

    private static string RequiredText(string? value, int maxLength, string label)
    {
        var text = value?.Trim();
        if (string.IsNullOrEmpty(text) || text.Length > maxLength)
        {
            throw AssetManagementException.Invalid(
                AssetMasterErrorCodes.Field,
                $"{label} wajib diisi, paling panjang {maxLength} karakter.");
        }

        return text;
    }

    private static string? OptionalText(string? value, int maxLength, string label)
    {
        var text = value?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        if (text.Length > maxLength)
        {
            throw AssetManagementException.Invalid(
                AssetMasterErrorCodes.Field,
                $"{label} paling panjang {maxLength} karakter.");
        }

        return text;
    }

    private static Guid RequiredId(Guid? value, string label)
    {
        if (value is not { } id || id == Guid.Empty)
        {
            throw AssetManagementException.Invalid(AssetMasterErrorCodes.Field, $"{label} wajib dipilih.");
        }

        return id;
    }

    private static AssetManagementException DuplicateCode() =>
        AssetManagementException.Conflict(AssetMasterErrorCodes.DuplicateCode, AssetMasterMessages.DuplicateCode);

    private static AssetManagementException NotFound(string label) =>
        AssetManagementException.NotFound(AssetMasterErrorCodes.NotFound, $"{label} tidak ditemukan atau sudah dihapus.");

    private static AssetManagementException InUse(string message) =>
        AssetManagementException.Invalid(AssetMasterErrorCodes.InUse, message);

    private static void StampCreate(global::QuilvianSystemBackend.Models.IdentityModel entity, Guid actorUserId)
    {
        entity.CreateBy = actorUserId;
        entity.CreateDateTime = DateTime.UtcNow;
    }

    private static void StampUpdate(global::QuilvianSystemBackend.Models.IdentityModel entity, Guid actorUserId)
    {
        entity.UpdateBy = actorUserId;
        entity.UpdateDateTime = DateTime.UtcNow;
    }

    private static void StampDelete(global::QuilvianSystemBackend.Models.IdentityModel entity, Guid actorUserId)
    {
        entity.IsDelete = true;
        entity.DeleteBy = actorUserId;
        entity.DeleteDateTime = DateTime.UtcNow;
    }

    private Task AuditAsync(string action, object data, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan master data aset.", new { Data = data, ActorUserId = actorUserId });

    private static AssetMasterFilterMetadataResponse BuildMetadata(
        string codeLabel,
        string nameLabel,
        IEnumerable<AssetMasterSortOptionResponse> extraSorts,
        IEnumerable<AssetMasterQueryParameterResponse> extraParameters,
        IEnumerable<AssetMasterOptionSourceResponse> flagOptions,
        IEnumerable<AssetMasterFormFieldResponse> fields)
    {
        var sorts = new List<AssetMasterSortOptionResponse>
        {
            new() { Value = "name", Label = nameLabel },
            new() { Value = "code", Label = codeLabel },
            new() { Value = "isActive", Label = "Status" },
            new() { Value = "createDateTime", Label = "Tanggal Dibuat" }
        };
        sorts.AddRange(extraSorts);

        var parameters = new List<AssetMasterQueryParameterResponse>
        {
            new() { Name = "search", Type = "string", Description = $"Mencari pada {codeLabel.ToLowerInvariant()} dan {nameLabel.ToLowerInvariant()}" },
            new() { Name = "isActive", Type = "boolean", Description = "true = aktif, false = nonaktif, kosong = semua", Example = "true" },
            new() { Name = "sortBy", Type = "string", Description = "Kolom pengurutan, lihat sortOptions", Example = "name" },
            new() { Name = "sortDirection", Type = "string", Description = "asc atau desc", Example = "asc" },
            new() { Name = "pageNumber", Type = "integer", Description = "Halaman, mulai 1", Example = "1" },
            new() { Name = "pageSize", Type = "integer", Description = $"Jumlah baris per halaman, maksimal {MaxPageSize}", Example = "25" }
        };
        parameters.InsertRange(2, extraParameters);

        var fieldList = fields.ToList();
        return new AssetMasterFilterMetadataResponse
        {
            SortOptions = sorts,
            FlagOptions = flagOptions.ToList(),
            QueryParameters = parameters,
            CreateFields = fieldList,
            UpdateFields = fieldList
        };
    }

    private static AssetMasterOptionSourceResponse YesNoFlag(string name, string label, string yesLabel, string noLabel) => new()
    {
        Name = name,
        Label = label,
        Options =
        [
            new AssetMasterSortOptionResponse { Value = "true", Label = yesLabel },
            new AssetMasterSortOptionResponse { Value = "false", Label = noLabel }
        ]
    };

    private static AssetMasterQueryParameterResponse BooleanParameter(string name, string description) =>
        new() { Name = name, Type = "boolean", Description = description, Example = "true" };

    private static AssetMasterQueryParameterResponse GuidParameter(string name, string description) =>
        new() { Name = name, Type = "guid", Description = description };

    private static AssetMasterFormFieldResponse Field(
        string name,
        string label,
        string inputType,
        bool required,
        int sortOrder,
        int? maxLength = null,
        string? optionsSource = null,
        string? description = null,
        string? example = null) => new()
    {
        Name = name,
        Label = label,
        Section = "Informasi Utama",
        InputType = inputType,
        IsRequiredOnCreate = required,
        IsRequiredOnUpdate = required,
        RequiredType = required ? "Required" : "Optional",
        MaxLength = maxLength,
        OptionsSource = optionsSource,
        Description = description,
        Example = example,
        SortOrder = sortOrder
    };
}

public static class AssetMasterErrorCodes
{
    public const string DuplicateCode = "AST-VAL-REG-050";
    public const string InventoryCodeFormat = "AST-VAL-REG-051";
    public const string InventoryCodeReserved = "AST-VAL-REG-052";
    public const string InventoryCodeLocked = "AST-VAL-REG-053";
    public const string LocationUnit = "AST-VAL-REG-055";
    public const string SettingRange = "AST-VAL-REG-056";
    public const string Field = "AST-MST-FIELD";
    public const string Reference = "AST-MST-REFERENCE";
    public const string NotFound = "AST-MST-NOT-FOUND";
    public const string InUse = "AST-MST-IN-USE";
}

public static class AssetMasterMessages
{
    public const string DuplicateCode = "Kode ini sudah dipakai data lain.";
    public const string InventoryCodeFormat = "Kode inventaris harus 2–11 huruf besar atau angka tanpa spasi.";
    public const string InventoryCodeReserved = "Kode ini dipakai untuk nomor dokumen aset. Pilih kode lain.";
    public const string InventoryCodeLocked = "Kode inventaris tidak dapat diubah karena sudah dipakai pada nomor aset.";
    public const string LocationUnit = "Unit pemilik ruangan tidak ditemukan atau sudah tidak aktif.";
    public const string SettingRange = "Nilai pengaturan di luar batas yang diizinkan.";
}
