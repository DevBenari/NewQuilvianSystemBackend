using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services
{
    public interface IHrdPersonnelActionOrderService
    {
        Task<PagedResult<PersonnelActionOrderListResponse>> GetOrdersAsync(
            int pageNumber,
            int pageSize,
            string? actionCategory,
            string? orderStatus,
            Guid? organizationUnitId,
            DateTime? startDate,
            DateTime? endDate,
            string? search,
            CancellationToken ct = default);

        Task<PersonnelActionOrderDetailResponse?> GetOrderByIdAsync(Guid id, CancellationToken ct = default);

        Task<PersonnelActionOrderSummaryResponse> GetSummaryAsync(CancellationToken ct = default);

        Task<PersonnelActionOrderFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken ct = default);

        Task<PersonnelActionOrderDetailResponse> CreateDraftOrderAsync(
            CreatePersonnelActionOrderRequest request,
            Guid userId,
            CancellationToken ct = default);

        Task<PersonnelActionOrderDetailResponse> UpdateDraftOrderAsync(
            Guid id,
            UpdatePersonnelActionOrderRequest request,
            Guid userId,
            CancellationToken ct = default);

        Task<AttachmentResponse> UploadAttachmentAsync(
            Guid id,
            string fileName,
            string originalFileName,
            string contentType,
            long fileSize,
            string storagePath,
            Guid userId,
            CancellationToken ct = default);

        Task<(string PhysicalPath, string FileName, string ContentType)?> GetAttachmentFileAsync(
            Guid id,
            Guid attachmentId,
            CancellationToken ct = default);

        Task<PersonnelActionOrderDetailResponse> SubmitOrderAsync(
            Guid id,
            Guid userId,
            CancellationToken ct = default);

        Task<PersonnelActionOrderDetailResponse> ApproveOrderAsync(
            Guid id,
            Guid userId,
            CancellationToken ct = default);

        Task<PersonnelActionOrderDetailResponse> RejectOrderAsync(
            Guid id,
            string reason,
            Guid userId,
            CancellationToken ct = default);

        Task<PersonnelActionOrderDetailResponse> CancelOrderAsync(
            Guid id,
            string reason,
            string? counterOrderNumber,
            Guid userId,
            CancellationToken ct = default);
    }
}
