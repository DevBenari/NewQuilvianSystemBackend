using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services
{
    public class HrServiceCatalogService : IHrServiceCatalogService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<HrServiceCatalogService> _logger;

        public HrServiceCatalogService(ApplicationDbContext dbContext, ILogger<HrServiceCatalogService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        private static Guid ParseGuid(string userId)
        {
            return Guid.TryParse(userId, out var g) ? g : Guid.Empty;
        }

        #region Service Categories

        public async Task<List<HrServiceCategoryDto>> GetActiveCategoriesAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.MstHrServiceCategories
                .Where(x => !x.IsDelete);

            if (!includeInactive)
            {
                query = query.Where(x => x.IsActive);
            }

            var categories = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ServiceCategoryName)
                .Select(x => new HrServiceCategoryDto
                {
                    Id = x.Id,
                    ServiceCategoryCode = x.ServiceCategoryCode,
                    ServiceCategoryName = x.ServiceCategoryName,
                    IconName = x.IconName,
                    DisplayColor = x.DisplayColor,
                    DefaultSlaHours = x.DefaultSlaHours,
                    SortOrder = x.SortOrder,
                    IsEmployeeVisible = x.IsEmployeeVisible,
                    IsManagerVisible = x.IsManagerVisible,
                    IsConfidentialByDefault = x.IsConfidentialByDefault,
                    IsActive = x.IsActive,
                    Description = x.Description,
                    TotalServiceTypes = x.ServiceTypes.Count(st => !st.IsDelete && st.IsActive)
                })
                .ToListAsync(cancellationToken);

            return categories;
        }

        public async Task<HrServiceCategoryDto?> GetCategoryByIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
        {
            var cat = await _dbContext.MstHrServiceCategories
                .Include(x => x.ServiceTypes)
                .FirstOrDefaultAsync(x => x.Id == categoryId && !x.IsDelete, cancellationToken);

            if (cat == null) return null;

            return new HrServiceCategoryDto
            {
                Id = cat.Id,
                ServiceCategoryCode = cat.ServiceCategoryCode,
                ServiceCategoryName = cat.ServiceCategoryName,
                IconName = cat.IconName,
                DisplayColor = cat.DisplayColor,
                DefaultSlaHours = cat.DefaultSlaHours,
                SortOrder = cat.SortOrder,
                IsEmployeeVisible = cat.IsEmployeeVisible,
                IsManagerVisible = cat.IsManagerVisible,
                IsConfidentialByDefault = cat.IsConfidentialByDefault,
                IsActive = cat.IsActive,
                Description = cat.Description,
                TotalServiceTypes = cat.ServiceTypes.Count(st => !st.IsDelete && st.IsActive)
            };
        }

        public async Task<HrServiceCategoryDto> CreateCategoryAsync(CreateHrServiceCategoryRequestDto request, string currentUserId, CancellationToken cancellationToken = default)
        {
            var exists = await _dbContext.MstHrServiceCategories
                .AnyAsync(x => x.ServiceCategoryCode.ToLower() == request.ServiceCategoryCode.ToLower() && !x.IsDelete, cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException($"Kategori dengan kode '{request.ServiceCategoryCode}' sudah terdaftar");
            }

            var actorGuid = ParseGuid(currentUserId);
            var category = new MstHrServiceCategory
            {
                Id = Guid.NewGuid(),
                ServiceCategoryCode = request.ServiceCategoryCode.Trim().ToUpperInvariant(),
                ServiceCategoryName = request.ServiceCategoryName.Trim(),
                IconName = request.IconName,
                DisplayColor = request.DisplayColor,
                DefaultSlaHours = request.DefaultSlaHours,
                SortOrder = request.SortOrder,
                IsEmployeeVisible = request.IsEmployeeVisible,
                IsManagerVisible = request.IsManagerVisible,
                IsConfidentialByDefault = request.IsConfidentialByDefault,
                IsActive = true,
                Description = request.Description,
                CreateBy = actorGuid,
                CreateDateTime = DateTime.UtcNow
            };

            _dbContext.MstHrServiceCategories.Add(category);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[CATALOG-HRS-01] Kategori layanan SDM baru '{CategoryName}' ({CategoryCode}) dibuat oleh {UserId}",
                category.ServiceCategoryName, category.ServiceCategoryCode, currentUserId);

            return (await GetCategoryByIdAsync(category.Id, cancellationToken))!;
        }

        public async Task<HrServiceCategoryDto> UpdateCategoryAsync(Guid categoryId, UpdateHrServiceCategoryRequestDto request, string currentUserId, CancellationToken cancellationToken = default)
        {
            var cat = await _dbContext.MstHrServiceCategories
                .FirstOrDefaultAsync(x => x.Id == categoryId && !x.IsDelete, cancellationToken);

            if (cat == null)
            {
                throw new InvalidOperationException($"Kategori layanan dengan ID {categoryId} tidak ditemukan");
            }

            var actorGuid = ParseGuid(currentUserId);
            cat.ServiceCategoryName = request.ServiceCategoryName.Trim();
            cat.IconName = request.IconName;
            cat.DisplayColor = request.DisplayColor;
            cat.DefaultSlaHours = request.DefaultSlaHours;
            cat.SortOrder = request.SortOrder;
            cat.IsEmployeeVisible = request.IsEmployeeVisible;
            cat.IsManagerVisible = request.IsManagerVisible;
            cat.IsConfidentialByDefault = request.IsConfidentialByDefault;
            cat.IsActive = request.IsActive;
            cat.Description = request.Description;
            cat.UpdateBy = actorGuid;
            cat.UpdateDateTime = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[CATALOG-HRS-02] Kategori layanan SDM '{CategoryName}' diperbarui oleh {UserId}",
                cat.ServiceCategoryName, currentUserId);

            return (await GetCategoryByIdAsync(cat.Id, cancellationToken))!;
        }

        public async Task<bool> DeleteCategoryAsync(Guid categoryId, string currentUserId, CancellationToken cancellationToken = default)
        {
            var cat = await _dbContext.MstHrServiceCategories
                .Include(x => x.ServiceTypes)
                .FirstOrDefaultAsync(x => x.Id == categoryId && !x.IsDelete, cancellationToken);

            if (cat == null) return false;

            if (cat.ServiceTypes.Any(st => !st.IsDelete))
            {
                throw new InvalidOperationException("Tidak dapat menghapus kategori yang masih memiliki tipe layanan aktif di dalamnya");
            }

            var actorGuid = ParseGuid(currentUserId);
            cat.IsDelete = true;
            cat.DeleteBy = actorGuid;
            cat.DeleteDateTime = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[CATALOG-HRS-03] Kategori layanan SDM ID {CategoryId} dihapus (soft-delete) oleh {UserId}", categoryId, currentUserId);
            return true;
        }

        #endregion

        #region Service Types

        public async Task<List<HrServiceTypeDto>> GetServiceTypesAsync(Guid? categoryId = null, bool activeOnly = true, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.MstHrServiceTypes
                .Include(x => x.HrServiceCategory)
                .Include(x => x.EmployeeDocumentType)
                .Where(x => !x.IsDelete);

            if (categoryId.HasValue)
            {
                query = query.Where(x => x.HrServiceCategoryId == categoryId.Value);
            }

            if (activeOnly)
            {
                query = query.Where(x => x.IsActive);
            }

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ServiceTypeName)
                .Select(x => new HrServiceTypeDto
                {
                    Id = x.Id,
                    HrServiceCategoryId = x.HrServiceCategoryId,
                    CategoryName = x.HrServiceCategory != null ? x.HrServiceCategory.ServiceCategoryName : string.Empty,
                    ServiceTypeCode = x.ServiceTypeCode,
                    ServiceTypeName = x.ServiceTypeName,
                    RequestClassification = x.RequestClassification,
                    DefaultPriority = x.DefaultPriority,
                    DefaultSlaHours = x.DefaultSlaHours,
                    FirstResponseSlaHours = x.FirstResponseSlaHours,
                    ResolutionSlaHours = x.ResolutionSlaHours,
                    DefaultAssignedRoleCode = x.DefaultAssignedRoleCode,
                    AssignmentSource = x.AssignmentSource,
                    RequiresWorkflow = x.RequiresWorkflow,
                    RequiresAttachment = x.RequiresAttachment,
                    AllowsEmployeeComment = x.AllowsEmployeeComment,
                    AllowsInternalComment = x.AllowsInternalComment,
                    AutoCreateDocumentRequest = x.AutoCreateDocumentRequest,
                    IsEmployeeSelectable = x.IsEmployeeSelectable,
                    IsConfidential = x.IsConfidential,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    Description = x.Description,
                    FormSchemaJson = x.FormSchemaJson,
                    ValidationRuleJson = x.ValidationRuleJson,
                    EmployeeDocumentTypeId = x.EmployeeDocumentTypeId,
                    DocumentTypeName = x.EmployeeDocumentType != null ? x.EmployeeDocumentType.DocumentTypeName : null
                })
                .ToListAsync(cancellationToken);

            return items;
        }

        public async Task<HrServiceTypeDto?> GetServiceTypeByIdAsync(Guid typeId, CancellationToken cancellationToken = default)
        {
            var item = await _dbContext.MstHrServiceTypes
                .Include(x => x.HrServiceCategory)
                .Include(x => x.EmployeeDocumentType)
                .FirstOrDefaultAsync(x => x.Id == typeId && !x.IsDelete, cancellationToken);

            if (item == null) return null;

            return new HrServiceTypeDto
            {
                Id = item.Id,
                HrServiceCategoryId = item.HrServiceCategoryId,
                CategoryName = item.HrServiceCategory != null ? item.HrServiceCategory.ServiceCategoryName : string.Empty,
                ServiceTypeCode = item.ServiceTypeCode,
                ServiceTypeName = item.ServiceTypeName,
                RequestClassification = item.RequestClassification,
                DefaultPriority = item.DefaultPriority,
                DefaultSlaHours = item.DefaultSlaHours,
                FirstResponseSlaHours = item.FirstResponseSlaHours,
                ResolutionSlaHours = item.ResolutionSlaHours,
                DefaultAssignedRoleCode = item.DefaultAssignedRoleCode,
                AssignmentSource = item.AssignmentSource,
                RequiresWorkflow = item.RequiresWorkflow,
                RequiresAttachment = item.RequiresAttachment,
                AllowsEmployeeComment = item.AllowsEmployeeComment,
                AllowsInternalComment = item.AllowsInternalComment,
                AutoCreateDocumentRequest = item.AutoCreateDocumentRequest,
                IsEmployeeSelectable = item.IsEmployeeSelectable,
                IsConfidential = item.IsConfidential,
                SortOrder = item.SortOrder,
                IsActive = item.IsActive,
                Description = item.Description,
                FormSchemaJson = item.FormSchemaJson,
                ValidationRuleJson = item.ValidationRuleJson,
                EmployeeDocumentTypeId = item.EmployeeDocumentTypeId,
                DocumentTypeName = item.EmployeeDocumentType != null ? item.EmployeeDocumentType.DocumentTypeName : null
            };
        }

        public async Task<HrServiceTypeDto> CreateServiceTypeAsync(CreateHrServiceTypeRequestDto request, string currentUserId, CancellationToken cancellationToken = default)
        {
            var categoryExists = await _dbContext.MstHrServiceCategories
                .AnyAsync(x => x.Id == request.HrServiceCategoryId && !x.IsDelete, cancellationToken);

            if (!categoryExists)
            {
                throw new InvalidOperationException($"Kategori layanan dengan ID {request.HrServiceCategoryId} tidak ditemukan");
            }

            var typeExists = await _dbContext.MstHrServiceTypes
                .AnyAsync(x => x.ServiceTypeCode.ToLower() == request.ServiceTypeCode.ToLower() && !x.IsDelete, cancellationToken);

            if (typeExists)
            {
                throw new InvalidOperationException($"Tipe layanan dengan kode '{request.ServiceTypeCode}' sudah ada");
            }

            var actorGuid = ParseGuid(currentUserId);
            var serviceType = new MstHrServiceType
            {
                Id = Guid.NewGuid(),
                HrServiceCategoryId = request.HrServiceCategoryId,
                ServiceTypeCode = request.ServiceTypeCode.Trim().ToUpperInvariant(),
                ServiceTypeName = request.ServiceTypeName.Trim(),
                RequestClassification = request.RequestClassification,
                DefaultPriority = request.DefaultPriority,
                DefaultSlaHours = request.DefaultSlaHours,
                FirstResponseSlaHours = request.FirstResponseSlaHours,
                ResolutionSlaHours = request.ResolutionSlaHours,
                DefaultAssignedRoleCode = request.DefaultAssignedRoleCode,
                AssignmentSource = request.AssignmentSource,
                RequiresWorkflow = request.RequiresWorkflow,
                RequiresAttachment = request.RequiresAttachment,
                AllowsEmployeeComment = request.AllowsEmployeeComment,
                AllowsInternalComment = request.AllowsInternalComment,
                AutoCreateDocumentRequest = request.AutoCreateDocumentRequest,
                IsEmployeeSelectable = request.IsEmployeeSelectable,
                IsConfidential = request.IsConfidential,
                SortOrder = request.SortOrder,
                IsActive = true,
                Description = request.Description,
                FormSchemaJson = request.FormSchemaJson,
                ValidationRuleJson = request.ValidationRuleJson,
                EmployeeDocumentTypeId = request.EmployeeDocumentTypeId,
                CreateBy = actorGuid,
                CreateDateTime = DateTime.UtcNow
            };

            _dbContext.MstHrServiceTypes.Add(serviceType);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[CATALOG-HRS-04] Tipe layanan SDM baru '{TypeName}' ({TypeCode}) berhasil dibuat",
                serviceType.ServiceTypeName, serviceType.ServiceTypeCode);

            return (await GetServiceTypeByIdAsync(serviceType.Id, cancellationToken))!;
        }

        public async Task<HrServiceTypeDto> UpdateServiceTypeAsync(Guid typeId, UpdateHrServiceTypeRequestDto request, string currentUserId, CancellationToken cancellationToken = default)
        {
            var item = await _dbContext.MstHrServiceTypes
                .FirstOrDefaultAsync(x => x.Id == typeId && !x.IsDelete, cancellationToken);

            if (item == null)
            {
                throw new InvalidOperationException($"Tipe layanan dengan ID {typeId} tidak ditemukan");
            }

            var actorGuid = ParseGuid(currentUserId);
            item.ServiceTypeName = request.ServiceTypeName.Trim();
            item.DefaultSlaHours = request.DefaultSlaHours;
            item.FirstResponseSlaHours = request.FirstResponseSlaHours;
            item.ResolutionSlaHours = request.ResolutionSlaHours;
            item.DefaultAssignedRoleCode = request.DefaultAssignedRoleCode;
            item.RequiresWorkflow = request.RequiresWorkflow;
            item.RequiresAttachment = request.RequiresAttachment;
            item.AllowsEmployeeComment = request.AllowsEmployeeComment;
            item.AllowsInternalComment = request.AllowsInternalComment;
            item.AutoCreateDocumentRequest = request.AutoCreateDocumentRequest;
            item.IsEmployeeSelectable = request.IsEmployeeSelectable;
            item.IsConfidential = request.IsConfidential;
            item.IsActive = request.IsActive;
            item.SortOrder = request.SortOrder;
            item.Description = request.Description;
            item.FormSchemaJson = request.FormSchemaJson;
            item.ValidationRuleJson = request.ValidationRuleJson;
            item.EmployeeDocumentTypeId = request.EmployeeDocumentTypeId;
            item.UpdateBy = actorGuid;
            item.UpdateDateTime = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[CATALOG-HRS-05] Tipe layanan SDM ID {TypeId} diperbarui", typeId);
            return (await GetServiceTypeByIdAsync(item.Id, cancellationToken))!;
        }

        public async Task<bool> DeleteServiceTypeAsync(Guid typeId, string currentUserId, CancellationToken cancellationToken = default)
        {
            var item = await _dbContext.MstHrServiceTypes
                .FirstOrDefaultAsync(x => x.Id == typeId && !x.IsDelete, cancellationToken);

            if (item == null) return false;

            var inUse = await _dbContext.TrxHrServiceRequests
                .AnyAsync(x => x.HrServiceTypeId == typeId && !x.IsDelete, cancellationToken);

            if (inUse)
            {
                throw new InvalidOperationException("Tipe layanan tidak dapat dihapus karena sudah memiliki tiket permohonan yang tercatat");
            }

            var actorGuid = ParseGuid(currentUserId);
            item.IsDelete = true;
            item.DeleteBy = actorGuid;
            item.DeleteDateTime = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        #endregion

        #region Document Types Template

        public async Task<List<EmployeeDocumentTypeDto>> GetDocumentTypesAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.MstEmployeeDocumentTypes
                .Where(x => !x.IsDelete);

            if (activeOnly)
            {
                query = query.Where(x => x.IsActive);
            }

            var list = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.DocumentTypeName)
                .Select(x => new EmployeeDocumentTypeDto
                {
                    Id = x.Id,
                    DocumentTypeCode = x.DocumentTypeCode,
                    DocumentTypeName = x.DocumentTypeName,
                    DocumentCategory = x.DocumentCategory,
                    NumberingPattern = x.NumberingPattern,
                    LastSequenceNumber = x.LastSequenceNumber,
                    CurrentYear = x.CurrentYear,
                    TemplateBodyHtml = x.TemplateBodyHtml,
                    SupportedTokensJson = x.SupportedTokensJson,
                    ValidityPeriodDays = x.ValidityPeriodDays,
                    IsFinancialSensitive = x.IsFinancialSensitive,
                    RequiresCbApproval = x.RequiresCbApproval,
                    AuthorizedSignerTitle = x.AuthorizedSignerTitle,
                    RequiresApproval = x.RequiresApproval,
                    RequiresDigitalSignature = x.RequiresDigitalSignature,
                    AllowsEmployeeDownload = x.AllowsEmployeeDownload,
                    IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken);

            return list;
        }

        public async Task<EmployeeDocumentTypeDto?> GetDocumentTypeByIdAsync(Guid documentTypeId, CancellationToken cancellationToken = default)
        {
            var x = await _dbContext.MstEmployeeDocumentTypes
                .FirstOrDefaultAsync(x => x.Id == documentTypeId && !x.IsDelete, cancellationToken);

            if (x == null) return null;

            return new EmployeeDocumentTypeDto
            {
                Id = x.Id,
                DocumentTypeCode = x.DocumentTypeCode,
                DocumentTypeName = x.DocumentTypeName,
                DocumentCategory = x.DocumentCategory,
                NumberingPattern = x.NumberingPattern,
                LastSequenceNumber = x.LastSequenceNumber,
                CurrentYear = x.CurrentYear,
                TemplateBodyHtml = x.TemplateBodyHtml,
                SupportedTokensJson = x.SupportedTokensJson,
                ValidityPeriodDays = x.ValidityPeriodDays,
                IsFinancialSensitive = x.IsFinancialSensitive,
                RequiresCbApproval = x.RequiresCbApproval,
                AuthorizedSignerTitle = x.AuthorizedSignerTitle,
                RequiresApproval = x.RequiresApproval,
                RequiresDigitalSignature = x.RequiresDigitalSignature,
                AllowsEmployeeDownload = x.AllowsEmployeeDownload,
                IsActive = x.IsActive
            };
        }

        public async Task<EmployeeDocumentTypeDto> CreateDocumentTypeAsync(CreateEmployeeDocumentTypeRequestDto request, string currentUserId, CancellationToken cancellationToken = default)
        {
            var exists = await _dbContext.MstEmployeeDocumentTypes
                .AnyAsync(x => x.DocumentTypeCode.ToLower() == request.DocumentTypeCode.ToLower() && !x.IsDelete, cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException($"Tipe dokumen dengan kode '{request.DocumentTypeCode}' sudah terdaftar");
            }

            var actorGuid = ParseGuid(currentUserId);
            var docType = new MstEmployeeDocumentType
            {
                Id = Guid.NewGuid(),
                DocumentTypeCode = request.DocumentTypeCode.Trim().ToUpperInvariant(),
                DocumentTypeName = request.DocumentTypeName.Trim(),
                DocumentCategory = request.DocumentCategory,
                NumberingPattern = request.NumberingPattern,
                LastSequenceNumber = 0,
                CurrentYear = DateTime.UtcNow.Year,
                TemplateBodyHtml = request.TemplateBodyHtml,
                SupportedTokensJson = request.SupportedTokensJson ?? "[]",
                ValidityPeriodDays = request.ValidityPeriodDays,
                IsFinancialSensitive = request.IsFinancialSensitive,
                RequiresCbApproval = request.RequiresCbApproval,
                AuthorizedSignerTitle = request.AuthorizedSignerTitle,
                RequiresApproval = request.RequiresApproval,
                RequiresDigitalSignature = request.RequiresDigitalSignature,
                AllowsEmployeeDownload = request.AllowsEmployeeDownload,
                IsActive = true,
                CreateBy = actorGuid,
                CreateDateTime = DateTime.UtcNow
            };

            _dbContext.MstEmployeeDocumentTypes.Add(docType);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[CATALOG-HRS-06] Master template naskah dinas '{DocName}' ({DocCode}) berhasil didaftarkan",
                docType.DocumentTypeName, docType.DocumentTypeCode);

            return (await GetDocumentTypeByIdAsync(docType.Id, cancellationToken))!;
        }

        #endregion
    }
}
