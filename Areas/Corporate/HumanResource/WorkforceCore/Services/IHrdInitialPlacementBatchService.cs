using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services
{
    public interface IHrdInitialPlacementBatchService
    {
        Task<List<InitialBatchRowResponse>> GetTemplateRowsAsync(CancellationToken ct = default);
        Task<BatchValidationResultResponse> ValidateRowsAsync(List<CreateInitialBatchRowDto> rows, CancellationToken ct = default);
        Task<InitialBatchDetailResponse> CreateBatchDraftAsync(CreateInitialBatchRequest request, Guid userId, CancellationToken ct = default);
        Task<PagedResult<InitialBatchListResponse>> GetBatchesAsync(int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<InitialBatchDetailResponse?> GetBatchByIdAsync(Guid id, CancellationToken ct = default);
        Task<InitialBatchDetailResponse?> ValidateBatchByIdAsync(Guid id, CancellationToken ct = default);
        Task<InitialBatchDetailResponse> ExecuteBatchAsync(Guid id, Guid userId, CancellationToken ct = default);
    }
}
