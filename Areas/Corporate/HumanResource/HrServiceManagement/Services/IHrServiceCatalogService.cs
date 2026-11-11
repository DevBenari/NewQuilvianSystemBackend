using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services
{
    public interface IHrServiceCatalogService
    {
        // Service Categories
        Task<List<HrServiceCategoryDto>> GetActiveCategoriesAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
        Task<HrServiceCategoryDto?> GetCategoryByIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
        Task<HrServiceCategoryDto> CreateCategoryAsync(CreateHrServiceCategoryRequestDto request, string currentUserId, CancellationToken cancellationToken = default);
        Task<HrServiceCategoryDto> UpdateCategoryAsync(Guid categoryId, UpdateHrServiceCategoryRequestDto request, string currentUserId, CancellationToken cancellationToken = default);
        Task<bool> DeleteCategoryAsync(Guid categoryId, string currentUserId, CancellationToken cancellationToken = default);

        // Service Types
        Task<List<HrServiceTypeDto>> GetServiceTypesAsync(Guid? categoryId = null, bool activeOnly = true, CancellationToken cancellationToken = default);
        Task<HrServiceTypeDto?> GetServiceTypeByIdAsync(Guid typeId, CancellationToken cancellationToken = default);
        Task<HrServiceTypeDto> CreateServiceTypeAsync(CreateHrServiceTypeRequestDto request, string currentUserId, CancellationToken cancellationToken = default);
        Task<HrServiceTypeDto> UpdateServiceTypeAsync(Guid typeId, UpdateHrServiceTypeRequestDto request, string currentUserId, CancellationToken cancellationToken = default);
        Task<bool> DeleteServiceTypeAsync(Guid typeId, string currentUserId, CancellationToken cancellationToken = default);

        // Document Types Template
        Task<List<EmployeeDocumentTypeDto>> GetDocumentTypesAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
        Task<EmployeeDocumentTypeDto?> GetDocumentTypeByIdAsync(Guid documentTypeId, CancellationToken cancellationToken = default);
        Task<EmployeeDocumentTypeDto> CreateDocumentTypeAsync(CreateEmployeeDocumentTypeRequestDto request, string currentUserId, CancellationToken cancellationToken = default);
    }
}
