using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;
using QuilvianSystemBackend.Hubs;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services
{
    /// <summary>
    /// Implementasi pusat pemberitahuan dalam aplikasi (BE-HRD-314, WF-B7, INT-HR-95).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Keputusan kanal (Gelombang 2, 10 November 2026).</b> Platform belum memiliki tabel notifikasi,
    /// pengirim email, maupun gateway WhatsApp. Kanal pertama yang dipilih adalah pemberitahuan dalam aplikasi:
    /// tersimpan di <c>HrdNotification</c>, dibaca lewat endpoint pusat notifikasi, dan didorong realtime lewat
    /// hub SignalR <see cref="HrNotificationHub"/> yang memakai infrastruktur SignalR yang sudah ada untuk antrean.
    /// Kanal lain yang diminta pemanggil dicatat pada kolom <c>RequestedChannel</c> tanpa mengubah penyaluran.
    /// </para>
    /// <para>
    /// <b>Penerima jabatan tanpa nama peran di kode.</b> <see cref="NotifyPermissionHoldersAsync"/> mencari
    /// akun aktif yang pasangan unit × jabatannya diberi kebijakan akses (<c>SysAccessPolicy</c>) untuk aksi
    /// yang disebut. Jadi siapa yang menerima "eskalasi ke SDM" ditentukan admin di layar Akses Role.
    /// </para>
    /// </remarks>
    public class HrdNotificationService : IHrdNotificationService
    {
        public const string ChannelInApp = "InApp";
        private const int MaxPageSize = 200;

        private readonly ApplicationDbContext _context;
        private readonly IHubContext<HrNotificationHub> _hubContext;
        private readonly ILogger<HrdNotificationService> _logger;

        public HrdNotificationService(
            ApplicationDbContext context,
            IHubContext<HrNotificationHub> hubContext,
            ILogger<HrdNotificationService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<int> NotifyUserAsync(Guid userId, HrdNotificationRequest request, CancellationToken cancellationToken = default)
        {
            return NotifyUsersAsync(new[] { userId }, request, cancellationToken);
        }

        public async Task<int> NotifyUsersAsync(IEnumerable<Guid> userIds, HrdNotificationRequest request, CancellationToken cancellationToken = default)
        {
            ValidateRequest(request);

            var distinctIds = userIds.Where(id => id != Guid.Empty).Distinct().ToList();
            if (distinctIds.Count == 0)
            {
                return 0;
            }

            var recipients = await _context.Users
                .AsNoTracking()
                .Where(u => distinctIds.Contains(u.Id) && u.IsActive)
                .Select(u => new { u.Id, u.WorkforceProfileId })
                .ToListAsync(cancellationToken);

            if (recipients.Count == 0)
            {
                return 0;
            }

            var now = DateTime.UtcNow;
            var created = new List<HrdNotification>();

            HashSet<string> existingKeys = new(StringComparer.Ordinal);
            if (!string.IsNullOrWhiteSpace(request.DedupKey))
            {
                var candidateKeys = recipients.Select(r => BuildDedupKey(request.DedupKey!, r.Id)).ToList();
                existingKeys = (await _context.HrdNotifications
                        .AsNoTracking()
                        .Where(n => n.DedupKey != null && candidateKeys.Contains(n.DedupKey))
                        .Select(n => n.DedupKey!)
                        .ToListAsync(cancellationToken))
                    .ToHashSet(StringComparer.Ordinal);
            }

            foreach (var recipient in recipients)
            {
                string? dedupKey = null;
                if (!string.IsNullOrWhiteSpace(request.DedupKey))
                {
                    dedupKey = BuildDedupKey(request.DedupKey!, recipient.Id);
                    if (existingKeys.Contains(dedupKey))
                    {
                        continue;
                    }
                }

                created.Add(new HrdNotification
                {
                    Id = Guid.NewGuid(),
                    RecipientUserId = recipient.Id,
                    RecipientWorkforceProfileId = recipient.WorkforceProfileId,
                    Title = Truncate(request.Title, 200),
                    Body = Truncate(request.Body, 2000),
                    Category = Truncate(request.Category, 60),
                    Priority = NormalizePriority(request.Priority),
                    Channel = ChannelInApp,
                    RequestedChannel = string.IsNullOrWhiteSpace(request.RequestedChannel) ? null : Truncate(request.RequestedChannel!, 30),
                    SourceModule = string.IsNullOrWhiteSpace(request.SourceModule) ? null : Truncate(request.SourceModule!, 80),
                    ReferenceType = string.IsNullOrWhiteSpace(request.ReferenceType) ? null : Truncate(request.ReferenceType!, 100),
                    ReferenceId = request.ReferenceId,
                    DeepLinkUrl = string.IsNullOrWhiteSpace(request.DeepLinkUrl) ? null : Truncate(request.DeepLinkUrl!, 300),
                    DedupKey = dedupKey,
                    IsRead = false,
                    DeliveredAt = now,
                    CreateDateTime = now,
                    CreateBy = request.ActorUserId ?? Guid.Empty
                });
            }

            if (created.Count == 0)
            {
                return 0;
            }

            _context.HrdNotifications.AddRange(created);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (!string.IsNullOrWhiteSpace(request.DedupKey))
            {
                // Dua worker atau dua request menulis kunci anti-duplikat yang sama pada saat bersamaan;
                // index unik menahan yang kedua. Itu memang hasil yang diinginkan, bukan kegagalan.
                _logger.LogInformation(ex, "Pemberitahuan dengan kunci anti-duplikat {DedupKey} sudah ada; pengiriman ganda ditahan.", request.DedupKey);
                foreach (var entry in created)
                {
                    _context.Entry(entry).State = EntityState.Detached;
                }

                return 0;
            }

            await PushRealtimeAsync(created, cancellationToken);
            return created.Count;
        }

        public async Task<int> NotifyWorkforceProfileAsync(Guid workforceProfileId, HrdNotificationRequest request, CancellationToken cancellationToken = default)
        {
            if (workforceProfileId == Guid.Empty)
            {
                return 0;
            }

            var userIds = await _context.Users
                .AsNoTracking()
                .Where(u => u.IsActive && u.WorkforceProfileId == workforceProfileId)
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);

            return await NotifyUsersAsync(userIds, request, cancellationToken);
        }

        public async Task<int> NotifyEmployeeAsync(Guid employeeId, HrdNotificationRequest request, CancellationToken cancellationToken = default)
        {
            if (employeeId == Guid.Empty)
            {
                return 0;
            }

            // Akun bisa terhubung lewat EmployeeId langsung, atau lewat profil tenaga kerja milik karyawan itu.
            var workforceProfileId = await _context.MstEmployees
                .AsNoTracking()
                .Where(e => e.Id == employeeId)
                .Select(e => (Guid?)e.WorkforceProfileId)
                .FirstOrDefaultAsync(cancellationToken);

            var userIds = await _context.Users
                .AsNoTracking()
                .Where(u => u.IsActive && (u.EmployeeId == employeeId || (workforceProfileId != null && u.WorkforceProfileId == workforceProfileId)))
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);

            return await NotifyUsersAsync(userIds, request, cancellationToken);
        }

        public async Task<int> NotifyPermissionHoldersAsync(string controllerName, string actionName, HrdNotificationRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(controllerName) || string.IsNullOrWhiteSpace(actionName))
            {
                return 0;
            }

            var policies = await (
                    from policy in _context.SysAccessPolicies.AsNoTracking()
                    join controller in _context.SysControllerAccesses.AsNoTracking() on policy.ControllerAccessId equals controller.Id
                    join action in _context.SysActionAccesses.AsNoTracking() on policy.ActionAccessId equals action.Id
                    where policy.IsAllowed && policy.IsActive && !policy.IsDelete
                          && controller.ControllerName == controllerName
                          && action.ActionName == actionName
                    select new { policy.DepartmentId, policy.PositionId })
                .Distinct()
                .ToListAsync(cancellationToken);

            if (policies.Count == 0)
            {
                _logger.LogWarning("Tidak ada kebijakan Akses Role untuk {Controller} : {Action}; pemberitahuan '{Title}' tidak punya penerima.", controllerName, actionName, request.Title);
                return 0;
            }

            var departmentIds = policies.Select(p => p.DepartmentId).Distinct().ToList();
            var candidates = await _context.Users
                .AsNoTracking()
                .Where(u => u.IsActive && u.PrimaryDepartmentId != null && u.PrimaryPositionId != null && departmentIds.Contains(u.PrimaryDepartmentId.Value))
                .Select(u => new { u.Id, u.PrimaryDepartmentId, u.PrimaryPositionId })
                .ToListAsync(cancellationToken);

            var allowedPairs = policies.Select(p => (p.DepartmentId, p.PositionId)).ToHashSet();
            var userIds = candidates
                .Where(u => allowedPairs.Contains((u.PrimaryDepartmentId!.Value, u.PrimaryPositionId!.Value)))
                .Select(u => u.Id)
                .ToList();

            return await NotifyUsersAsync(userIds, request, cancellationToken);
        }

        public async Task<PagedResult<HrdNotificationDto>> GetMyNotificationsAsync(Guid userId, bool? unreadOnly, string? category, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > MaxPageSize) pageSize = MaxPageSize;

            var now = DateTime.UtcNow;
            var query = _context.HrdNotifications
                .AsNoTracking()
                .Where(n => n.RecipientUserId == userId && !n.IsDelete && (n.ExpiresAt == null || n.ExpiresAt > now));

            if (unreadOnly == true)
            {
                query = query.Where(n => !n.IsRead);
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                var normalized = category.Trim().ToUpperInvariant();
                query = query.Where(n => n.Category == normalized);
            }

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(n => n.DeliveredAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(n => ToDto(n))
                .ToListAsync(cancellationToken);

            return new PagedResult<HrdNotificationDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            };
        }

        public async Task<HrdNotificationUnreadCountDto> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var count = await _context.HrdNotifications
                .AsNoTracking()
                .CountAsync(n => n.RecipientUserId == userId && !n.IsRead && !n.IsDelete && (n.ExpiresAt == null || n.ExpiresAt > now), cancellationToken);

            return new HrdNotificationUnreadCountDto { UnreadCount = count, CheckedAt = now };
        }

        public async Task<HrdNotificationMarkReadResultDto> MarkReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
        {
            var notification = await _context.HrdNotifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientUserId == userId && !n.IsDelete, cancellationToken);

            if (notification == null)
            {
                throw new KeyNotFoundException("Pemberitahuan tidak ditemukan atau bukan milik Anda.");
            }

            var now = DateTime.UtcNow;
            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = now;
                notification.UpdateDateTime = now;
                notification.UpdateBy = userId;
                await _context.SaveChangesAsync(cancellationToken);
                return new HrdNotificationMarkReadResultDto { AffectedCount = 1, MarkedAt = now };
            }

            return new HrdNotificationMarkReadResultDto { AffectedCount = 0, MarkedAt = now };
        }

        public async Task<HrdNotificationMarkReadResultDto> MarkAllReadAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var unread = await _context.HrdNotifications
                .Where(n => n.RecipientUserId == userId && !n.IsRead && !n.IsDelete)
                .ToListAsync(cancellationToken);

            foreach (var notification in unread)
            {
                notification.IsRead = true;
                notification.ReadAt = now;
                notification.UpdateDateTime = now;
                notification.UpdateBy = userId;
            }

            if (unread.Count > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return new HrdNotificationMarkReadResultDto { AffectedCount = unread.Count, MarkedAt = now };
        }

        private async Task PushRealtimeAsync(List<HrdNotification> created, CancellationToken cancellationToken)
        {
            var pushed = 0;
            foreach (var notification in created)
            {
                try
                {
                    await _hubContext.Clients
                        .Group(HrNotificationHub.UserGroupName(notification.RecipientUserId))
                        .SendAsync(HrNotificationHub.NotificationReceivedMethod, ToDto(notification), cancellationToken);
                    notification.IsPushedRealtime = true;
                    pushed++;
                }
                catch (Exception ex)
                {
                    // Dorongan realtime hanya kenyamanan; baris sudah tersimpan dan tetap terbaca lewat endpoint.
                    _logger.LogDebug(ex, "Dorongan realtime pemberitahuan {Id} gagal; penerima tetap melihatnya di pusat notifikasi.", notification.Id);
                }
            }

            if (pushed > 0)
            {
                try
                {
                    await _context.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Penandaan IsPushedRealtime gagal disimpan; tidak memengaruhi isi pemberitahuan.");
                }
            }
        }

        private static string BuildDedupKey(string dedupKey, Guid userId)
        {
            var key = $"{dedupKey.Trim()}|{userId:N}";
            return key.Length <= 250 ? key : key[^250..];
        }

        private static void ValidateRequest(HrdNotificationRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Title)) throw new ArgumentException("Judul pemberitahuan wajib diisi.", nameof(request));
            if (string.IsNullOrWhiteSpace(request.Body)) throw new ArgumentException("Isi pemberitahuan wajib diisi.", nameof(request));
            if (string.IsNullOrWhiteSpace(request.Category)) throw new ArgumentException("Kategori pemberitahuan wajib diisi.", nameof(request));
        }

        private static string NormalizePriority(string? priority)
        {
            return priority?.Trim() switch
            {
                HrdNotificationPriorities.High => HrdNotificationPriorities.High,
                HrdNotificationPriorities.Critical => HrdNotificationPriorities.Critical,
                _ => HrdNotificationPriorities.Normal
            };
        }

        private static string Truncate(string value, int maxLength)
        {
            value = value.Trim();
            return value.Length <= maxLength ? value : value[..maxLength];
        }

        private static HrdNotificationDto ToDto(HrdNotification n) => new()
        {
            Id = n.Id,
            Title = n.Title,
            Body = n.Body,
            Category = n.Category,
            Priority = n.Priority,
            Channel = n.Channel,
            SourceModule = n.SourceModule,
            ReferenceType = n.ReferenceType,
            ReferenceId = n.ReferenceId,
            DeepLinkUrl = n.DeepLinkUrl,
            IsRead = n.IsRead,
            ReadAt = n.ReadAt,
            DeliveredAt = n.DeliveredAt
        };
    }
}
