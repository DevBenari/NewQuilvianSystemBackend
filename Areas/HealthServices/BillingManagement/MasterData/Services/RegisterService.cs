using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;

public sealed class RegisterService
{
    private const string LogCategory = "HealthServices.BillingManagement.MasterData";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public RegisterService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    public async Task<PagedResult<RegisterResponse>> GetPagedAsync(
        string? search,
        bool? isActive,
        string? sortBy,
        string? sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

        var query = BuildBaseQuery();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToLower();
            query = query.Where(x =>
                x.RegisterCode.ToLower().Contains(keyword) ||
                x.RegisterName.ToLower().Contains(keyword) ||
                (x.Location != null && x.Location.ToLower().Contains(keyword)));
        }
        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

        var totalData = await query.CountAsync(cancellationToken);
        var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (sortBy ?? "registerName").ToLowerInvariant() switch
        {
            "registercode" => isDesc ? query.OrderByDescending(x => x.RegisterCode) : query.OrderBy(x => x.RegisterCode),
            "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
            _ => isDesc ? query.OrderByDescending(x => x.RegisterName) : query.OrderBy(x => x.RegisterName)
        };

        var entities = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var items = entities.Select(ToResponse).ToList();

        return new PagedResult<RegisterResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = totalData,
            TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
            Items = items
        };
    }

    public async Task<IReadOnlyList<RegisterOptionResponse>> GetOptionsAsync(
        bool onlyActive,
        string? search,
        CancellationToken cancellationToken)
    {
        var query = BuildBaseQuery();
        if (onlyActive) query = query.Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToLower();
            query = query.Where(x =>
                x.RegisterCode.ToLower().Contains(keyword) ||
                x.RegisterName.ToLower().Contains(keyword));
        }

        return await query
            .OrderBy(x => x.RegisterName)
            .Select(x => new RegisterOptionResponse
            {
                Id = x.Id,
                RegisterCode = x.RegisterCode,
                RegisterName = x.RegisterName,
                Location = x.Location
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<RegisterResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await BuildBaseQuery()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Register tidak ditemukan.");
        return ToResponse(entity);
    }

    public async Task<bool> ExistsActiveAsync(Guid id, CancellationToken cancellationToken)
    {
        return await BuildBaseQuery().AnyAsync(x => x.Id == id && x.IsActive, cancellationToken);
    }

    public async Task<RegisterResponse> CreateAsync(
        CreateRegisterRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        await ValidateAsync(null, request.RegisterCode, request.RegisterName, cancellationToken);

        var entity = new MstRegister
        {
            Id = Guid.NewGuid(),
            RegisterCode = request.RegisterCode.Trim().ToUpperInvariant(),
            RegisterName = request.RegisterName.Trim(),
            Location = NormalizeNullableText(request.Location),
            Description = NormalizeNullableText(request.Description),
            IsActive = request.IsActive,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };

        _dbContext.Set<MstRegister>().Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = ToResponse(entity);
        await _loggerService.InfoAsync(
            LogCategory, "Register.Create", "Membuat data register kasir.", result);
        return result;
    }

    public async Task<RegisterResponse> UpdateAsync(
        Guid id,
        UpdateRegisterRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Set<MstRegister>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Register tidak ditemukan.");

        await ValidateAsync(id, request.RegisterCode, request.RegisterName, cancellationToken);

        entity.RegisterCode = request.RegisterCode.Trim().ToUpperInvariant();
        entity.RegisterName = request.RegisterName.Trim();
        entity.Location = NormalizeNullableText(request.Location);
        entity.Description = NormalizeNullableText(request.Description);
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(entity);
    }

    public async Task<RegisterStatusResponse> ChangeStatusAsync(
        Guid id,
        bool isActive,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Set<MstRegister>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Register tidak ditemukan.");

        entity.IsActive = isActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RegisterStatusResponse
        {
            Id = entity.Id,
            RegisterCode = entity.RegisterCode,
            RegisterName = entity.RegisterName,
            IsActive = entity.IsActive
        };
    }

    public async Task<RegisterDeleteResponse> DeleteAsync(
        Guid id,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Set<MstRegister>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Register tidak ditemukan.");

        entity.IsDelete = true;
        entity.IsActive = false;
        entity.DeleteDateTime = DateTime.UtcNow;
        entity.DeleteBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RegisterDeleteResponse
        {
            Id = entity.Id,
            RegisterCode = entity.RegisterCode,
            RegisterName = entity.RegisterName,
            IsDelete = entity.IsDelete
        };
    }

    private IQueryable<MstRegister> BuildBaseQuery() =>
        _dbContext.Set<MstRegister>().AsNoTracking().Where(x => !x.IsDelete);

    private async Task ValidateAsync(
        Guid? excludeId,
        string registerCode,
        string registerName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(registerCode))
            throw new RegisterValidationException("Kode register wajib diisi.");
        if (string.IsNullOrWhiteSpace(registerName))
            throw new RegisterValidationException("Nama register wajib diisi.");

        var normalizedCode = registerCode.Trim().ToUpperInvariant();
        var normalizedName = registerName.Trim().ToLower();

        var duplicateCode = await _dbContext.Set<MstRegister>().AnyAsync(
            x => !x.IsDelete && x.RegisterCode.ToUpper() == normalizedCode
                && (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken);
        if (duplicateCode)
            throw new RegisterValidationException("Kode register sudah digunakan.");

        var duplicateName = await _dbContext.Set<MstRegister>().AnyAsync(
            x => !x.IsDelete && x.RegisterName.ToLower() == normalizedName
                && (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken);
        if (duplicateName)
            throw new RegisterValidationException("Nama register sudah digunakan.");
    }

    private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 25;
        if (pageSize > 100) pageSize = 100;
        return (pageNumber, pageSize);
    }

    private static string? NormalizeNullableText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static RegisterResponse ToResponse(MstRegister x) => new()
    {
        Id = x.Id,
        RegisterCode = x.RegisterCode,
        RegisterName = x.RegisterName,
        Location = x.Location,
        Description = x.Description,
        IsActive = x.IsActive,
        CreateDateTime = x.CreateDateTime
    };
}

public sealed class RegisterValidationException(string message) : Exception(message);
