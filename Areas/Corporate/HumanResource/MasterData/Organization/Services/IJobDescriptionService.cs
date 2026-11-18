using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Services
{
    public interface IJobDescriptionService
    {
        Task<JobDescriptionFilterMetadataResponse> GetFilterMetadataAsync();
        Task<JobDescriptionSummaryResponse> GetSummaryAsync();
        Task<PagedResult<JobDescriptionResponse>> GetDataAsync(
            Guid? positionId,
            Guid? departmentId,
            string? status,
            DateTime? asOfDate,
            string? search,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize);
        Task<JobDescriptionDetailResponse?> GetByIdAsync(Guid id);
        Task<JobDescriptionDetailResponse?> GetEffectiveByPositionAsync(Guid positionId, DateTime? asOfDate);
        Task<List<JobDescriptionHistoryResponse>> GetHistoryByPositionAsync(Guid positionId);
        Task<(bool Success, string? ErrorMessage, int StatusCode, JobDescriptionDetailResponse? Result)> CreateDraftAsync(CreateJobDescriptionDraftRequest request, Guid actorId);
        Task<(bool Success, string? ErrorMessage, int StatusCode, JobDescriptionDetailResponse? Result)> UpdateDraftAsync(Guid id, UpdateJobDescriptionDraftRequest request, Guid actorId);
        Task<(bool Success, string? ErrorMessage, int StatusCode, JobDescriptionDetailResponse? Result)> ActivateVersionAsync(Guid id, ActivateJobDescriptionRequest? request, Guid actorId);
        Task<(bool Success, string? ErrorMessage, int StatusCode)> DeactivateAsync(Guid id, Guid actorId);
        Task<(bool Success, string? ErrorMessage, int StatusCode)> DeleteAsync(Guid id, Guid actorId);
    }
}
