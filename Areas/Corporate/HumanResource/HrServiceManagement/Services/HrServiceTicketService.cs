using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services
{
    public class HrServiceTicketService : IHrServiceTicketService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDocumentAutoNumberingService _numberingService;
        private readonly IHrServiceSlaEngine _slaEngine;
        private readonly IWorkforceProfileSyncService _profileSyncService;
        private readonly ILogger<HrServiceTicketService> _logger;

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".jpg", ".jpeg", ".png", ".docx"
        };

        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        public HrServiceTicketService(
            ApplicationDbContext dbContext,
            IDocumentAutoNumberingService numberingService,
            IHrServiceSlaEngine slaEngine,
            IWorkforceProfileSyncService profileSyncService,
            ILogger<HrServiceTicketService> logger)
        {
            _dbContext = dbContext;
            _numberingService = numberingService;
            _slaEngine = slaEngine;
            _profileSyncService = profileSyncService;
            _logger = logger;
        }

        private static Guid ParseGuid(string userId)
        {
            return Guid.TryParse(userId, out var g) ? g : Guid.Empty;
        }

        #region ESS Pegawai

        public async Task<HrServiceTicketDto> SubmitTicketEssAsync(Guid workforceProfileId, CreateHrServiceTicketRequestDto request, string currentUserId, CancellationToken cancellationToken = default)
        {
            var profile = await _dbContext.MstWorkforceProfiles
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete, cancellationToken);

            if (profile == null)
            {
                throw new InvalidOperationException($"Profil pegawai dengan ID {workforceProfileId} tidak ditemukan");
            }

            var serviceType = await _dbContext.MstHrServiceTypes
                .Include(x => x.HrServiceCategory)
                .FirstOrDefaultAsync(x => x.Id == request.HrServiceTypeId && x.HrServiceCategoryId == request.HrServiceCategoryId && !x.IsDelete && x.IsActive, cancellationToken);

            if (serviceType == null)
            {
                throw new InvalidOperationException("Kategori atau jenis layanan yang dipilih tidak aktif atau tidak valid");
            }

            // Validasi Aturan Bisnis HRS-B1 & VR-HRS-02:
            // Tiket dengan prioritas Cito wajib menyertakan surat disposisi atau bukti panggilan dinas darurat
            if (string.Equals(request.PriorityTier, "Cito", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(request.EmergencyDispositionReference))
                {
                    throw new InvalidOperationException("[VR-HRS-02] Pengajuan permohonan darurat prioritas Cito wajib menyertakan nomor referensi surat disposisi pimpinan medis/keperawatan");
                }
            }

            // Hitung target jam SLA berdasarkan Tier
            int slaHours = request.PriorityTier.ToLowerInvariant() switch
            {
                "cito" => 4,
                "khusus" => 72,
                _ => serviceType.DefaultSlaHours
            };

            var now = DateTime.UtcNow;
            // Gunakan HrServiceSlaEngine untuk menghitung batas waktu SLA (HRS-B2)
            var deadlineUtc = _slaEngine.CalculateSlaDeadline(now, request.PriorityTier, slaHours);
            var ticketNumber = await _numberingService.GenerateTicketNumberAsync(cancellationToken);

            var actorGuid = ParseGuid(currentUserId);

            var ticket = new TrxHrServiceRequest
            {
                Id = Guid.NewGuid(),
                RequestNumber = ticketNumber,
                HrServiceCategoryId = request.HrServiceCategoryId,
                HrServiceTypeId = request.HrServiceTypeId,
                RequestedByWorkforceProfileId = workforceProfileId,
                RequestedByUserId = actorGuid,
                Subject = request.Subject.Trim(),
                RequestDescription = request.RequestDescription.Trim(),
                Priority = request.PriorityTier == "Cito" ? "Urgent" : (request.PriorityTier == "Khusus" ? "High" : "Normal"),
                PriorityTier = request.PriorityTier,
                SlaTargetHours = slaHours,
                SlaDeadlineUtc = deadlineUtc,
                SlaDueAt = deadlineUtc,
                SlaStatus = "WithinSla",
                RequestStatus = "Submitted",
                SourceChannel = "ESS-Web",
                RequestedAt = now,
                SubmittedAt = now,
                ExternalReferenceNumber = request.EmergencyDispositionReference ?? request.ExternalReferenceNumber,
                RequestPayloadJson = request.RequestPayloadJson,
                IsEmployeeVisible = true,
                IsActive = true,
                CreateBy = actorGuid,
                CreateDateTime = now
            };

            _dbContext.TrxHrServiceRequests.Add(ticket);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[TICKET-HRS-01] Tiket helpdesk SDM baru berhasil disubmit: {TicketNumber} (Tier: {Tier}, SLA: {Hours} Jam, Deadline: {Deadline}) oleh Pegawai {ProfileId}",
                ticket.RequestNumber, ticket.PriorityTier, ticket.SlaTargetHours, ticket.SlaDeadlineUtc, workforceProfileId);

            return (await GetTicketDetailAsync(ticket.Id, cancellationToken: cancellationToken))!;
        }

        public async Task<List<HrServiceTicketDto>> GetMyTicketsAsync(Guid workforceProfileId, CancellationToken cancellationToken = default)
        {
            var tickets = await _dbContext.TrxHrServiceRequests
                .Include(x => x.HrServiceCategory)
                .Include(x => x.HrServiceType)
                .Include(x => x.RequestedByWorkforceProfile)
                .Include(x => x.AssignedToUser)
                .Where(x => x.RequestedByWorkforceProfileId == workforceProfileId && !x.IsDelete)
                .OrderByDescending(x => x.RequestedAt)
                .Select(x => new HrServiceTicketDto
                {
                    Id = x.Id,
                    RequestNumber = x.RequestNumber,
                    HrServiceCategoryId = x.HrServiceCategoryId,
                    CategoryName = x.HrServiceCategory != null ? x.HrServiceCategory.ServiceCategoryName : string.Empty,
                    HrServiceTypeId = x.HrServiceTypeId,
                    ServiceTypeName = x.HrServiceType != null ? x.HrServiceType.ServiceTypeName : string.Empty,
                    RequestedByWorkforceProfileId = x.RequestedByWorkforceProfileId,
                    EmployeeName = x.RequestedByWorkforceProfile != null ? x.RequestedByWorkforceProfile.DisplayName : string.Empty,
                    EmployeeCode = x.RequestedByWorkforceProfile != null ? x.RequestedByWorkforceProfile.ProfileCode : string.Empty,
                    Subject = x.Subject,
                    RequestDescription = x.RequestDescription,
                    RequestStatus = x.RequestStatus,
                    PriorityTier = x.PriorityTier,
                    SlaTargetHours = x.SlaTargetHours,
                    SlaDeadlineUtc = x.SlaDeadlineUtc,
                    SlaStatus = x.SlaStatus,
                    AssignedToUserId = x.AssignedToUserId,
                    AssignedToUserName = x.AssignedToUser != null ? x.AssignedToUser.UserName : null,
                    RequestedAt = x.RequestedAt,
                    FirstResponseAt = x.FirstResponseAt,
                    ResolvedAt = x.ResolvedAt,
                    ResolutionSummary = x.ResolutionSummary,
                    TotalAttachments = x.Attachments.Count(a => !a.IsDelete),
                    TotalComments = x.Comments.Count(c => !c.IsDelete && !c.IsInternalNote)
                })
                .ToListAsync(cancellationToken);

            return tickets;
        }

        public async Task<HrServiceTicketDto?> GetTicketDetailAsync(Guid ticketId, Guid? requestingWorkforceProfileId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.TrxHrServiceRequests
                .Include(x => x.HrServiceCategory)
                .Include(x => x.HrServiceType)
                .Include(x => x.RequestedByWorkforceProfile)
                .Include(x => x.AssignedToUser)
                .Include(x => x.Attachments)
                .Include(x => x.Comments)
                .Where(x => x.Id == ticketId && !x.IsDelete);

            if (requestingWorkforceProfileId.HasValue)
            {
                query = query.Where(x => x.RequestedByWorkforceProfileId == requestingWorkforceProfileId.Value);
            }

            var x = await query.FirstOrDefaultAsync(cancellationToken);
            if (x == null) return null;

            return new HrServiceTicketDto
            {
                Id = x.Id,
                RequestNumber = x.RequestNumber,
                HrServiceCategoryId = x.HrServiceCategoryId,
                CategoryName = x.HrServiceCategory != null ? x.HrServiceCategory.ServiceCategoryName : string.Empty,
                HrServiceTypeId = x.HrServiceTypeId,
                ServiceTypeName = x.HrServiceType != null ? x.HrServiceType.ServiceTypeName : string.Empty,
                RequestedByWorkforceProfileId = x.RequestedByWorkforceProfileId,
                EmployeeName = x.RequestedByWorkforceProfile != null ? x.RequestedByWorkforceProfile.DisplayName : string.Empty,
                EmployeeCode = x.RequestedByWorkforceProfile != null ? x.RequestedByWorkforceProfile.ProfileCode : string.Empty,
                Subject = x.Subject,
                RequestDescription = x.RequestDescription,
                RequestStatus = x.RequestStatus,
                PriorityTier = x.PriorityTier,
                SlaTargetHours = x.SlaTargetHours,
                SlaDeadlineUtc = x.SlaDeadlineUtc,
                SlaStatus = x.SlaStatus,
                AssignedToUserId = x.AssignedToUserId,
                AssignedToUserName = x.AssignedToUser != null ? x.AssignedToUser.UserName : null,
                RequestedAt = x.RequestedAt,
                FirstResponseAt = x.FirstResponseAt,
                ResolvedAt = x.ResolvedAt,
                ResolutionSummary = x.ResolutionSummary,
                TotalAttachments = x.Attachments.Count(a => !a.IsDelete),
                TotalComments = x.Comments.Count(c => !c.IsDelete)
            };
        }

        #endregion

        #region Meja Kerja Helpdesk HR

        public async Task<List<HrServiceTicketDto>> GetAllTicketsQueueAsync(string? statusFilter = null, string? priorityTier = null, Guid? assignedToUserId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.TrxHrServiceRequests
                .Include(x => x.HrServiceCategory)
                .Include(x => x.HrServiceType)
                .Include(x => x.RequestedByWorkforceProfile)
                .Include(x => x.AssignedToUser)
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                query = query.Where(x => x.RequestStatus == statusFilter);
            }

            if (!string.IsNullOrWhiteSpace(priorityTier))
            {
                query = query.Where(x => x.PriorityTier == priorityTier);
            }

            if (assignedToUserId.HasValue)
            {
                query = query.Where(x => x.AssignedToUserId == assignedToUserId.Value);
            }

            var list = await query
                .OrderByDescending(x => x.PriorityTier == "Cito")
                .ThenBy(x => x.SlaDeadlineUtc)
                .Select(x => new HrServiceTicketDto
                {
                    Id = x.Id,
                    RequestNumber = x.RequestNumber,
                    HrServiceCategoryId = x.HrServiceCategoryId,
                    CategoryName = x.HrServiceCategory != null ? x.HrServiceCategory.ServiceCategoryName : string.Empty,
                    HrServiceTypeId = x.HrServiceTypeId,
                    ServiceTypeName = x.HrServiceType != null ? x.HrServiceType.ServiceTypeName : string.Empty,
                    RequestedByWorkforceProfileId = x.RequestedByWorkforceProfileId,
                    EmployeeName = x.RequestedByWorkforceProfile != null ? x.RequestedByWorkforceProfile.DisplayName : string.Empty,
                    EmployeeCode = x.RequestedByWorkforceProfile != null ? x.RequestedByWorkforceProfile.ProfileCode : string.Empty,
                    Subject = x.Subject,
                    RequestDescription = x.RequestDescription,
                    RequestStatus = x.RequestStatus,
                    PriorityTier = x.PriorityTier,
                    SlaTargetHours = x.SlaTargetHours,
                    SlaDeadlineUtc = x.SlaDeadlineUtc,
                    SlaStatus = x.SlaStatus,
                    AssignedToUserId = x.AssignedToUserId,
                    AssignedToUserName = x.AssignedToUser != null ? x.AssignedToUser.UserName : null,
                    RequestedAt = x.RequestedAt,
                    FirstResponseAt = x.FirstResponseAt,
                    ResolvedAt = x.ResolvedAt,
                    ResolutionSummary = x.ResolutionSummary,
                    TotalAttachments = x.Attachments.Count(a => !a.IsDelete),
                    TotalComments = x.Comments.Count(c => !c.IsDelete)
                })
                .ToListAsync(cancellationToken);

            return list;
        }

        public async Task<HrServiceTicketDto> AssignTicketAsync(Guid ticketId, AssignTicketRequestDto request, string currentUserId, CancellationToken cancellationToken = default)
        {
            var ticket = await _dbContext.TrxHrServiceRequests
                .FirstOrDefaultAsync(x => x.Id == ticketId && !x.IsDelete, cancellationToken);

            if (ticket == null)
            {
                throw new InvalidOperationException($"Tiket dengan ID {ticketId} tidak ditemukan");
            }

            var userExists = await _dbContext.Users.AnyAsync(u => u.Id == request.AssignedToUserId, cancellationToken);
            if (!userExists)
            {
                throw new InvalidOperationException($"User analis tujuan ID {request.AssignedToUserId} tidak ditemukan");
            }

            var actorGuid = ParseGuid(currentUserId);
            ticket.AssignedToUserId = request.AssignedToUserId;
            if (ticket.RequestStatus == "Submitted")
            {
                ticket.RequestStatus = "Assigned";
            }

            if (!ticket.FirstResponseAt.HasValue)
            {
                ticket.FirstResponseAt = DateTime.UtcNow;
            }

            ticket.UpdateBy = actorGuid;
            ticket.UpdateDateTime = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[TICKET-HRS-02] Tiket {TicketNumber} ditugaskan ke analis User ID: {UserId} oleh {Actor}",
                ticket.RequestNumber, request.AssignedToUserId, currentUserId);

            return (await GetTicketDetailAsync(ticket.Id, cancellationToken: cancellationToken))!;
        }

        public async Task<HrServiceTicketDto> UpdateTicketStatusAsync(Guid ticketId, UpdateTicketStatusRequestDto request, string currentUserId, CancellationToken cancellationToken = default)
        {
            var ticket = await _dbContext.TrxHrServiceRequests
                .FirstOrDefaultAsync(x => x.Id == ticketId && !x.IsDelete, cancellationToken);

            if (ticket == null)
            {
                throw new InvalidOperationException($"Tiket dengan ID {ticketId} tidak ditemukan");
            }

            var now = DateTime.UtcNow;
            var actorGuid = ParseGuid(currentUserId);
            var previousStatus = ticket.RequestStatus;

            // Aturan Bisnis HRS-W2: Penangguhan Timer pada Status Klarifikasi
            if (request.NewStatus == "WaitingStaffResponse" && previousStatus != "WaitingStaffResponse")
            {
                _slaEngine.PauseSlaTimer(ticket, now);
            }
            else if (previousStatus == "WaitingStaffResponse" && request.NewStatus != "WaitingStaffResponse")
            {
                _slaEngine.ResumeSlaTimer(ticket, now);
            }

            ticket.RequestStatus = request.NewStatus;
            ticket.UpdateBy = actorGuid;
            ticket.UpdateDateTime = now;

            if (request.NewStatus == "Resolved" || request.NewStatus == "Approved")
            {
                ticket.ResolvedAt = now;
                ticket.ResolutionSummary = request.ResolutionOrReasonNotes;

                // INT-HR-65: Jika tiket terkait pemutakhiran profil, sinkronkan data profil ke Sub-02 secara otomatis
                if (!string.IsNullOrWhiteSpace(ticket.RequestPayloadJson) && ticket.RequestPayloadJson != "{}")
                {
                    try
                    {
                        await _profileSyncService.ApplyApprovedProfileChangesAsync(ticket.Id, actorGuid, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[INT-HR-65] Peringatan: Sinkronisasi pemutakhiran profil untuk tiket {TicketId} gagal: {Message}",
                            ticket.Id, ex.Message);
                    }
                }
            }
            else if (request.NewStatus == "Closed")
            {
                ticket.ClosedAt = now;
            }
            else if (request.NewStatus == "Cancelled")
            {
                ticket.CancelledAt = now;
                ticket.CancellationReason = request.ResolutionOrReasonNotes;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[TICKET-HRS-03] Status tiket {TicketNumber} diubah dari '{Prev}' menjadi '{NewStatus}' oleh {UserId}",
                ticket.RequestNumber, previousStatus, request.NewStatus, currentUserId);

            return (await GetTicketDetailAsync(ticket.Id, cancellationToken: cancellationToken))!;
        }

        #endregion

        #region Komentar & Utas Klarifikasi

        public async Task<TicketCommentDto> AddCommentAsync(Guid ticketId, AddTicketCommentRequestDto request, Guid authorUserId, string authorName, bool isHrStaff, CancellationToken cancellationToken = default)
        {
            var ticket = await _dbContext.TrxHrServiceRequests
                .FirstOrDefaultAsync(x => x.Id == ticketId && !x.IsDelete, cancellationToken);

            if (ticket == null)
            {
                throw new InvalidOperationException($"Tiket dengan ID {ticketId} tidak ditemukan");
            }

            // Jika bukan staf HR, dilarang membuat internal note
            bool internalNote = isHrStaff && request.IsInternalNote;

            var now = DateTime.UtcNow;
            var comment = new TrxHrServiceRequestComment
            {
                Id = Guid.NewGuid(),
                HrServiceRequestId = ticketId,
                CommentByUserId = authorUserId,
                CommentText = request.CommentText.Trim(),
                IsInternalNote = internalNote,
                CommentedAt = now,
                CreateBy = authorUserId,
                CreateDateTime = now
            };

            _dbContext.TrxHrServiceRequestComments.Add(comment);

            // Jika staf pemohon membalas pesan klarifikasi, resume status tiket dari WaitingStaffResponse ke InReview (HRS-W2)
            if (!isHrStaff && ticket.RequestStatus == "WaitingStaffResponse")
            {
                _slaEngine.ResumeSlaTimer(ticket, now);
                ticket.RequestStatus = "InReview";
                ticket.LastActivityAt = now;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[COMMENT-HRS-01] Komentar baru ditambahkan pada tiket {TicketNumber} oleh {AuthorName} (Internal: {IsInternal})",
                ticket.RequestNumber, authorName, internalNote);

            return new TicketCommentDto
            {
                Id = comment.Id,
                RequestId = comment.HrServiceRequestId,
                AuthorUserId = comment.CommentByUserId ?? Guid.Empty,
                AuthorName = authorName,
                CommentText = comment.CommentText,
                IsInternalNote = comment.IsInternalNote,
                CreatedAtUtc = comment.CommentedAt
            };
        }

        public async Task<List<TicketCommentDto>> GetCommentsAsync(Guid ticketId, bool isHrStaff, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.TrxHrServiceRequestComments
                .Include(x => x.CommentByUser)
                .Where(x => x.HrServiceRequestId == ticketId && !x.IsDelete);

            // Jika bukan staf HR, sembunyikan catatan internal (HRS-W2)
            if (!isHrStaff)
            {
                query = query.Where(x => !x.IsInternalNote);
            }

            var comments = await query
                .OrderBy(x => x.CommentedAt)
                .Select(x => new TicketCommentDto
                {
                    Id = x.Id,
                    RequestId = x.HrServiceRequestId,
                    AuthorUserId = x.CommentByUserId ?? Guid.Empty,
                    AuthorName = x.CommentByUser != null ? x.CommentByUser.UserName ?? "User" : "User",
                    CommentText = x.CommentText,
                    IsInternalNote = x.IsInternalNote,
                    CreatedAtUtc = x.CommentedAt
                })
                .ToListAsync(cancellationToken);

            return comments;
        }

        #endregion

        #region Berkas Lampiran Tiket (BE-HRD-196)

        public async Task<TicketAttachmentDto> UploadAttachmentAsync(Guid ticketId, IFormFile file, string? description, Guid uploadedByUserId, Guid? workforceProfileId, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
            {
                throw new InvalidOperationException("Berkas lampiran tidak boleh kosong");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                throw new InvalidOperationException("[VR-HRS-15] Ukuran berkas melebihi batas maksimal 5MB");
            }

            var ext = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(ext))
            {
                throw new InvalidOperationException($"[VR-HRS-15] Format file '{ext}' tidak diizinkan. Format yang didukung: PDF, JPG, PNG, DOCX");
            }

            var ticket = await _dbContext.TrxHrServiceRequests
                .FirstOrDefaultAsync(x => x.Id == ticketId && !x.IsDelete, cancellationToken);

            if (ticket == null)
            {
                throw new InvalidOperationException($"Tiket dengan ID {ticketId} tidak ditemukan");
            }

            // Direktori penyimpanan lokal aman
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "hr-services", "attachments");
            Directory.CreateDirectory(uploadsDir);

            var safeFileName = $"{ticket.RequestNumber}_{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadsDir, safeFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            var now = DateTime.UtcNow;
            var attachment = new TrxHrServiceRequestAttachment
            {
                Id = Guid.NewGuid(),
                HrServiceRequestId = ticketId,
                UploadedByUserId = uploadedByUserId,
                UploadedByWorkforceProfileId = workforceProfileId,
                FileName = file.FileName,
                FilePath = fullPath,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                DocumentCategory = "SupportingDocument",
                Description = description,
                IsEmployeeVisible = true,
                UploadedAt = now,
                CreateBy = uploadedByUserId,
                CreateDateTime = now
            };

            _dbContext.TrxHrServiceRequestAttachments.Add(attachment);

            // Jika staf pemohon mengunggah lampiran perbaikan saat tiket berstatus WaitingStaffResponse, resume status tiket
            if (ticket.RequestStatus == "WaitingStaffResponse" && workforceProfileId == ticket.RequestedByWorkforceProfileId)
            {
                _slaEngine.ResumeSlaTimer(ticket, now);
                ticket.RequestStatus = "InReview";
                ticket.LastActivityAt = now;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[ATTACH-HRS-01] Berkas '{FileName}' ({Size} bytes) berhasil diunggah pada tiket {TicketNumber}",
                file.FileName, file.Length, ticket.RequestNumber);

            return new TicketAttachmentDto
            {
                Id = attachment.Id,
                RequestId = attachment.HrServiceRequestId,
                OriginalFileName = attachment.FileName,
                ContentType = attachment.ContentType ?? "application/octet-stream",
                FileSizeBytes = attachment.FileSizeBytes,
                AttachmentType = attachment.DocumentCategory ?? "SupportingDocument",
                UploadedAtUtc = attachment.UploadedAt
            };
        }

        public async Task<(byte[] FileBytes, string ContentType, string FileName)> DownloadAttachmentAsync(Guid ticketId, Guid attachmentId, Guid? requestingWorkforceProfileId, bool isHrStaff, CancellationToken cancellationToken = default)
        {
            var attachment = await _dbContext.TrxHrServiceRequestAttachments
                .Include(x => x.HrServiceRequest)
                .FirstOrDefaultAsync(x => x.Id == attachmentId && x.HrServiceRequestId == ticketId && !x.IsDelete, cancellationToken);

            if (attachment == null)
            {
                throw new InvalidOperationException("Berkas lampiran tidak ditemukan");
            }

            // Pengecekan otorisasi: pegawai biasa hanya boleh mengunduh lampiran pada tiket miliknya sendiri
            if (!isHrStaff && requestingWorkforceProfileId.HasValue)
            {
                if (attachment.HrServiceRequest?.RequestedByWorkforceProfileId != requestingWorkforceProfileId.Value)
                {
                    throw new UnauthorizedAccessException("Anda tidak memiliki hak akses untuk mengunduh lampiran pada tiket ini");
                }
            }

            if (!File.Exists(attachment.FilePath))
            {
                throw new FileNotFoundException("Berkas fisik lampiran tidak ditemukan pada repositori server");
            }

            var bytes = await File.ReadAllBytesAsync(attachment.FilePath, cancellationToken);
            var contentType = string.IsNullOrWhiteSpace(attachment.ContentType) ? "application/octet-stream" : attachment.ContentType;

            return (bytes, contentType, attachment.FileName);
        }

        public async Task<List<TicketAttachmentDto>> GetAttachmentsAsync(Guid ticketId, bool isHrStaff, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.TrxHrServiceRequestAttachments
                .Where(x => x.HrServiceRequestId == ticketId && !x.IsDelete);

            if (!isHrStaff)
            {
                query = query.Where(x => x.IsEmployeeVisible);
            }

            var list = await query
                .OrderByDescending(x => x.UploadedAt)
                .Select(x => new TicketAttachmentDto
                {
                    Id = x.Id,
                    RequestId = x.HrServiceRequestId,
                    OriginalFileName = x.FileName,
                    ContentType = x.ContentType ?? "application/octet-stream",
                    FileSizeBytes = x.FileSizeBytes,
                    AttachmentType = x.DocumentCategory ?? "SupportingDocument",
                    UploadedAtUtc = x.UploadedAt
                })
                .ToListAsync(cancellationToken);

            return list;
        }

        #endregion
    }
}
