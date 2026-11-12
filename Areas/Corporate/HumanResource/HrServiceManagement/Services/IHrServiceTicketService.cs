using Microsoft.AspNetCore.Http;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services
{
    public interface IHrServiceTicketService
    {
        // ESS Pegawai
        Task<HrServiceTicketDto> SubmitTicketEssAsync(Guid workforceProfileId, CreateHrServiceTicketRequestDto request, string currentUserId, CancellationToken cancellationToken = default);
        Task<List<HrServiceTicketDto>> GetMyTicketsAsync(Guid workforceProfileId, CancellationToken cancellationToken = default);
        Task<HrServiceTicketDto?> GetTicketDetailAsync(Guid ticketId, Guid? requestingWorkforceProfileId = null, CancellationToken cancellationToken = default);

        // Meja Kerja Helpdesk HR
        Task<List<HrServiceTicketDto>> GetAllTicketsQueueAsync(string? statusFilter = null, string? priorityTier = null, Guid? assignedToUserId = null, CancellationToken cancellationToken = default);
        Task<HrServiceTicketDto> AssignTicketAsync(Guid ticketId, AssignTicketRequestDto request, string currentUserId, CancellationToken cancellationToken = default);
        Task<HrServiceTicketDto> UpdateTicketStatusAsync(Guid ticketId, UpdateTicketStatusRequestDto request, string currentUserId, CancellationToken cancellationToken = default);

        // Komentar & Utas Klarifikasi
        Task<TicketCommentDto> AddCommentAsync(Guid ticketId, AddTicketCommentRequestDto request, Guid authorUserId, string authorName, bool isHrStaff, CancellationToken cancellationToken = default);
        Task<List<TicketCommentDto>> GetCommentsAsync(Guid ticketId, bool isHrStaff, CancellationToken cancellationToken = default);

        // Berkas Lampiran Tiket (BE-HRD-196)
        Task<TicketAttachmentDto> UploadAttachmentAsync(Guid ticketId, IFormFile file, string? description, Guid uploadedByUserId, Guid? workforceProfileId, CancellationToken cancellationToken = default);
        Task<(byte[] FileBytes, string ContentType, string FileName)> DownloadAttachmentAsync(Guid ticketId, Guid attachmentId, Guid? requestingWorkforceProfileId, bool isHrStaff, CancellationToken cancellationToken = default);
        Task<List<TicketAttachmentDto>> GetAttachmentsAsync(Guid ticketId, bool isHrStaff, CancellationToken cancellationToken = default);
    }
}
